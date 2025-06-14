using Azure;
using Azure.Communication.Email;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Settings;
using DerpCode.API.Services;
using DerpCode.API.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DerpCode.API.Tests.Services;

/// <summary>
/// Tests for the AcsEmailService class
/// </summary>
public sealed class AcsEmailServiceTests
{
    private readonly Mock<IOptions<EmailSettings>> mockEmailOptions;

    private readonly Mock<IOptions<AuthenticationSettings>> mockAuthOptions;

    private readonly Mock<IAcsEmailClientFactory> mockEmailClientFactory;

    private readonly Mock<ILogger<AcsEmailService>> mockLogger;

    private readonly Mock<IEmailTemplateService> mockEmailTemplateService;

    private readonly Mock<EmailClient> mockEmailClient;

    private readonly EmailSettings emailSettings;

    private readonly AuthenticationSettings authSettings = new AuthenticationSettings
    {
        UIBaseUrl = new Uri("https://test.derpcode.dev/")
    };

    private readonly AcsEmailService acsEmailService;

    public AcsEmailServiceTests()
    {
        this.mockEmailOptions = new Mock<IOptions<EmailSettings>>();
        this.mockAuthOptions = new Mock<IOptions<AuthenticationSettings>>();
        this.mockEmailClientFactory = new Mock<IAcsEmailClientFactory>();
        this.mockLogger = new Mock<ILogger<AcsEmailService>>();
        this.mockEmailClient = new Mock<EmailClient>();
        this.mockEmailTemplateService = new Mock<IEmailTemplateService>();

        this.emailSettings = new EmailSettings
        {
            FromAddress = "test@derpcode.dev",
            AcsEndpoint = new Uri("https://test-acs.communication.azure.com"),
            Enabled = true
        };

        this.mockEmailOptions.Setup(x => x.Value).Returns(this.emailSettings);
        this.mockAuthOptions.Setup(x => x.Value).Returns(this.authSettings);
        this.mockEmailClientFactory.Setup(x => x.CreateClient(It.IsAny<Azure.Core.TokenCredential>())).Returns(this.mockEmailClient.Object);

        this.acsEmailService = new AcsEmailService(
            this.mockEmailOptions.Object,
            this.mockEmailClientFactory.Object,
            this.mockEmailTemplateService.Object,
            this.mockAuthOptions.Object,
            this.mockLogger.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullEmailSettings_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new AcsEmailService(null!, this.mockEmailClientFactory.Object, this.mockEmailTemplateService.Object, this.mockAuthOptions.Object, this.mockLogger.Object));

        Assert.Equal("emailSettings", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullEmailClientFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new AcsEmailService(this.mockEmailOptions.Object, null!, this.mockEmailTemplateService.Object, this.mockAuthOptions.Object, this.mockLogger.Object));

        Assert.Equal("emailClientFactory", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new AcsEmailService(this.mockEmailOptions.Object, this.mockEmailClientFactory.Object, this.mockEmailTemplateService.Object, this.mockAuthOptions.Object, null!));

        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullEmailOptionsValue_ThrowsArgumentNullException()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<EmailSettings>>();
        mockOptions.Setup(x => x.Value).Returns((EmailSettings)null!);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new AcsEmailService(mockOptions.Object, this.mockEmailClientFactory.Object, this.mockEmailTemplateService.Object, this.mockAuthOptions.Object, this.mockLogger.Object));

        Assert.Equal("emailSettings", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new AcsEmailService(this.mockEmailOptions.Object, this.mockEmailClientFactory.Object, this.mockEmailTemplateService.Object, this.mockAuthOptions.Object, this.mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region SendEmailToUserAsync Tests

    [Fact]
    public async Task SendEmailToUserAsync_WithNullUser_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.acsEmailService.SendEmailToUserAsync(null!, "Subject", "Plain text", "HTML", CancellationToken.None));
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithNullSubject_ThrowsArgumentNullException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.acsEmailService.SendEmailToUserAsync(user, null!, "Plain text", "HTML", CancellationToken.None));
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithEmptySubject_ThrowsArgumentException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await this.acsEmailService.SendEmailToUserAsync(user, string.Empty, "Plain text", "HTML", CancellationToken.None));
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithNullPlainTextMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.acsEmailService.SendEmailToUserAsync(user, "Subject", null!, "HTML", CancellationToken.None));
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithEmptyPlainTextMessage_ThrowsArgumentException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await this.acsEmailService.SendEmailToUserAsync(user, "Subject", string.Empty, "HTML", CancellationToken.None));
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithNullHtmlMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.acsEmailService.SendEmailToUserAsync(user, "Subject", "Plain text", null!, CancellationToken.None));
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithEmptyHtmlMessage_ThrowsArgumentException()
    {
        // Arrange
        var user = CreateTestUser();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await this.acsEmailService.SendEmailToUserAsync(user, "Subject", "Plain text", string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithValidParameters_SendsEmail()
    {
        // Arrange
        var user = CreateTestUser();
        var subject = "Test Subject";
        var plainText = "Plain text message";
        var html = "<html><body>HTML message</body></html>";
        var cancellationToken = CancellationToken.None;

        var mockOperation = new Mock<EmailSendOperation>();

        this.mockEmailClient
            .Setup(x => x.SendAsync(
                WaitUntil.Started,
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockOperation.Object);

        // Act
        await this.acsEmailService.SendEmailToUserAsync(user, subject, plainText, html, cancellationToken);

        // Assert
        this.mockEmailClientFactory.Verify(x => x.CreateClient(null), Times.Once);
        this.mockEmailClient.Verify(x => x.SendAsync(
            WaitUntil.Started,
            It.Is<EmailMessage>(msg =>
                msg.SenderAddress == this.emailSettings.FromAddress &&
                msg.Content.Subject == subject &&
                msg.Content.PlainText == plainText &&
                msg.Content.Html == html &&
                msg.Recipients.To.Count == 1 &&
                msg.Recipients.To[0].Address == user.Email),
            cancellationToken), Times.Once);

        this.VerifyLoggerWasCalled(LogLevel.Debug, $"Sending email to user {user.Id} with subject {subject}");
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithCancellationToken_PassesToEmailClient()
    {
        // Arrange
        var user = CreateTestUser();
        var cancellationToken = new CancellationToken(true);

        this.mockEmailClient
            .Setup(x => x.SendAsync(
                WaitUntil.Started,
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await this.acsEmailService.SendEmailToUserAsync(user, "Subject", "Plain text", "HTML", cancellationToken));

        this.mockEmailClient.Verify(x => x.SendAsync(
            WaitUntil.Started,
            It.IsAny<EmailMessage>(),
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithEmailClientException_LogsErrorAndRethrows()
    {
        // Arrange
        var user = CreateTestUser();
        var subject = "Test Subject";
        var plainText = "Plain text message";
        var html = "<html><body>HTML message</body></html>";
        var expectedException = new RequestFailedException("Email service unavailable");

        this.mockEmailClient
            .Setup(x => x.SendAsync(
                WaitUntil.Started,
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<RequestFailedException>(async () =>
            await this.acsEmailService.SendEmailToUserAsync(user, subject, plainText, html, CancellationToken.None));

        Assert.Same(expectedException, actualException);

        this.VerifyLoggerWasCalled(LogLevel.Debug, $"Sending email to user {user.Id} with subject {subject}");
        this.VerifyLoggerWasCalled(LogLevel.Error, $"Error when sending email to {user.Id} with subject {subject}");
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithGenericException_LogsErrorAndRethrows()
    {
        // Arrange
        var user = CreateTestUser();
        var subject = "Test Subject";
        var plainText = "Plain text message";
        var html = "<html><body>HTML message</body></html>";
        var expectedException = new InvalidOperationException("Unexpected error");

        this.mockEmailClient
            .Setup(x => x.SendAsync(
                WaitUntil.Started,
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await this.acsEmailService.SendEmailToUserAsync(user, subject, plainText, html, CancellationToken.None));

        Assert.Same(expectedException, actualException);

        this.VerifyLoggerWasCalled(LogLevel.Debug, $"Sending email to user {user.Id} with subject {subject}");
        this.VerifyLoggerWasCalled(LogLevel.Error, $"Error when sending email to {user.Id} with subject {subject}");
    }

    [Fact]
    public async Task SendEmailToUserAsync_CreatesEmailMessageWithCorrectStructure()
    {
        // Arrange
        var user = CreateTestUser();
        var subject = "Welcome to DerpCode";
        var plainText = "Welcome to our platform!";
        var html = "<html><body><h1>Welcome to our platform!</h1></body></html>";

        EmailMessage? capturedMessage = null;
        this.mockEmailClient
            .Setup(x => x.SendAsync(
                WaitUntil.Started,
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .Callback<WaitUntil, EmailMessage, CancellationToken>((waitUntil, message, ct) => capturedMessage = message)
            .ReturnsAsync(new Mock<EmailSendOperation>().Object);

        // Act
        await this.acsEmailService.SendEmailToUserAsync(user, subject, plainText, html, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedMessage);
        Assert.Equal(this.emailSettings.FromAddress, capturedMessage.SenderAddress);
        Assert.Equal(subject, capturedMessage.Content.Subject);
        Assert.Equal(plainText, capturedMessage.Content.PlainText);
        Assert.Equal(html, capturedMessage.Content.Html);
        Assert.Single(capturedMessage.Recipients.To);
        Assert.Equal(user.Email, capturedMessage.Recipients.To[0].Address);
    }

    [Fact]
    public async Task SendEmailToUserAsync_WithSpecialCharactersInContent_HandlesCorrectly()
    {
        // Arrange
        var user = CreateTestUser();
        var subject = "Special Characters: àáâãäåæçèéêë";
        var plainText = "Content with special chars: £€¥¢™®©";
        var html = "<html><body>HTML with entities: &lt;&gt;&amp;&quot;</body></html>";

        var mockOperation = new Mock<EmailSendOperation>();
        this.mockEmailClient
            .Setup(x => x.SendAsync(
                WaitUntil.Started,
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockOperation.Object);

        // Act
        await this.acsEmailService.SendEmailToUserAsync(user, subject, plainText, html, CancellationToken.None);

        // Assert - Should not throw and should send the email
        this.mockEmailClient.Verify(x => x.SendAsync(
            WaitUntil.Started,
            It.Is<EmailMessage>(msg =>
                msg.Content.Subject == subject &&
                msg.Content.PlainText == plainText &&
                msg.Content.Html == html),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("   subject   ", "   plain   ", "   html   ")]
    [InlineData("subject\n\r", "plain\n\r", "html\n\r")]
    public async Task SendEmailToUserAsync_WithWhitespaceInContent_PreservesContent(string subject, string plainText, string html)
    {
        // Arrange
        var user = CreateTestUser();

        var mockOperation = new Mock<EmailSendOperation>();
        this.mockEmailClient
            .Setup(x => x.SendAsync(
                WaitUntil.Started,
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockOperation.Object);

        // Act
        await this.acsEmailService.SendEmailToUserAsync(user, subject, plainText, html, CancellationToken.None);

        // Assert - Content should be preserved as-is
        this.mockEmailClient.Verify(x => x.SendAsync(
            WaitUntil.Started,
            It.Is<EmailMessage>(msg =>
                msg.Content.Subject == subject &&
                msg.Content.PlainText == plainText &&
                msg.Content.Html == html),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helper Methods

    private static User CreateTestUser()
    {
        return new User
        {
            Id = 123,
            UserName = "testuser",
            Email = "test@example.com",
            EmailConfirmed = true
        };
    }

    private void VerifyLoggerWasCalled(LogLevel logLevel, string message)
    {
        this.mockLogger.Verify(
            x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}