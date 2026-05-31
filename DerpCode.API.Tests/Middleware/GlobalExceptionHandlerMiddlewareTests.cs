using System.IO;
using System.Text;
using System.Text.Json;
using DerpCode.API.Middleware;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Tests.Middleware;

public sealed class GlobalExceptionHandlerMiddlewareTests
{
    private const string LeakySensitiveMessage = "FATAL: relation \"users\" does not exist at position 42 (Npgsql.PostgresException)";

    private const string SafeFiveHundredFragment = "An unexpected error occurred while processing the request";

    private const string SafeFiveZeroFourFragment = "The request timed out";

    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> mockLogger;

    private readonly GlobalExceptionHandlerMiddleware middleware;

    public GlobalExceptionHandlerMiddlewareTests()
    {
        this.mockLogger = new Mock<ILogger<GlobalExceptionHandlerMiddleware>>();
        this.middleware = new GlobalExceptionHandlerMiddleware(_ => Task.CompletedTask, this.mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new GlobalExceptionHandlerMiddleware(_ => Task.CompletedTask, null!));

        Assert.Equal("logger", exception.ParamName);
    }

    #endregion

    #region InvokeAsync Tests

    [Fact]
    public async Task InvokeAsync_WithNullContext_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.middleware.InvokeAsync(null!));
    }

    [Fact]
    public async Task InvokeAsync_WithNoExceptionFeature_DoesNothing()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await this.middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
        this.mockLogger.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvokeAsync_WithGenericException_Returns500WithSanitizedBody()
    {
        // Arrange — a realistic-looking EF/Postgres error message that we MUST NOT echo back.
        var inner = new InvalidOperationException("connection refused to db-host-internal:5432");
        var exception = new InvalidOperationException(LeakySensitiveMessage, inner);
        var context = CreateHttpContextWithException(exception);

        // Act
        await this.middleware.InvokeAsync(context);

        // Assert — status and content type
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);

        var body = ReadBody(context);
        Assert.Contains(SafeFiveHundredFragment, body);

        // CRITICAL: raw exception messages and inner-exception messages must NOT leak to clients.
        Assert.DoesNotContain(LeakySensitiveMessage, body);
        Assert.DoesNotContain("connection refused to db-host-internal", body);
        Assert.DoesNotContain("Npgsql", body);
        Assert.DoesNotContain("PostgresException", body);

        // A correlation ID must still be present so users can quote it for support.
        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("correlationId", out var correlationId));
        Assert.False(string.IsNullOrWhiteSpace(correlationId.GetString()));
    }

    [Fact]
    public async Task InvokeAsync_WithTimeoutException_Returns504WithSanitizedBody()
    {
        // Arrange
        var exception = new TimeoutException(LeakySensitiveMessage);
        var context = CreateHttpContextWithException(exception);

        // Act
        await this.middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(StatusCodes.Status504GatewayTimeout, context.Response.StatusCode);

        var body = ReadBody(context);
        Assert.Contains(SafeFiveZeroFourFragment, body);
        Assert.DoesNotContain(LeakySensitiveMessage, body);
    }

    [Fact]
    public async Task InvokeAsync_LogsFullExceptionViaErrorLevel()
    {
        // Arrange
        var exception = new InvalidOperationException(LeakySensitiveMessage);
        var context = CreateHttpContextWithException(exception);

        // Act
        await this.middleware.InvokeAsync(context);

        // Assert — the exception object must be threaded into the logger so structured log sinks
        // (App Insights / OpenTelemetry) capture the full stack and inner-exception chain. This is
        // the operator's source of truth for diagnosis once the client message is sanitized.
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_PreservesCallerCorrelationIdInResponse()
    {
        // Arrange
        const string callerCorrelationId = "abc-123-deadbeef";
        var exception = new InvalidOperationException("anything");
        var context = CreateHttpContextWithException(exception);
        context.Request.Headers["X-Correlation-Id"] = callerCorrelationId;

        // Act
        await this.middleware.InvokeAsync(context);

        // Assert
        var body = ReadBody(context);
        using var doc = JsonDocument.Parse(body);
        Assert.Equal(callerCorrelationId, doc.RootElement.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseAlreadyStarted_DoesNotWriteBody()
    {
        // Arrange
        var exception = new InvalidOperationException("anything");
        var context = CreateHttpContextWithException(exception);

        var fakeResponseFeature = new FakeStartedResponseFeature();
        context.Features.Set<IHttpResponseFeature>(fakeResponseFeature);

        // Act
        await this.middleware.InvokeAsync(context);

        // Assert — status/content-type are still set on the response object, but no body bytes are
        // written because doing so would corrupt an in-flight response.
        Assert.Equal(0, fakeResponseFeature.Body.Length);
    }

    #endregion

    #region Helpers

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Method = "GET";
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("example.test");
        context.Request.Path = "/api/v1/anything";
        return context;
    }

    private static HttpContext CreateHttpContextWithException(Exception exception)
    {
        var context = CreateHttpContext();
        context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature
        {
            Error = exception,
            Path = context.Request.Path
        });
        return context;
    }

    private static string ReadBody(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        return reader.ReadToEnd();
    }

    private sealed class FakeStartedResponseFeature : IHttpResponseFeature
    {
        public Stream Body { get; set; } = new MemoryStream();

        public bool HasStarted => true;

        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();

        public string? ReasonPhrase { get; set; }

        public int StatusCode { get; set; } = StatusCodes.Status200OK;

        public void OnCompleted(Func<object, Task> callback, object state) { }

        public void OnStarting(Func<object, Task> callback, object state) { }
    }

    #endregion
}
