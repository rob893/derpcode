using System.Text.Json;
using DerpCode.API.Models;
using DerpCode.API.Models.Entities;
using DerpCode.API.Services;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace DerpCode.API.Tests.Services;

public class CodeExecutionServiceTests : IDisposable
{
    private readonly Mock<IDockerClient> mockDockerClient;

    private readonly Mock<ILogger<CodeExecutionService>> mockLogger;

    private readonly Mock<IContainerOperations> mockContainerOperations;

    private readonly Mock<IFileSystemService> mockFileSystemService;

    private readonly CodeExecutionService codeExecutionService;

    public CodeExecutionServiceTests()
    {
        this.mockDockerClient = new Mock<IDockerClient>();
        this.mockLogger = new Mock<ILogger<CodeExecutionService>>();
        this.mockContainerOperations = new Mock<IContainerOperations>();
        this.mockFileSystemService = new Mock<IFileSystemService>();

        this.mockDockerClient.Setup(x => x.Containers).Returns(this.mockContainerOperations.Object);

        this.codeExecutionService = new CodeExecutionService(this.mockDockerClient.Object, this.mockLogger.Object, this.mockFileSystemService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDockerClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CodeExecutionService(null!, this.mockLogger.Object, this.mockFileSystemService.Object));

        Assert.Equal("dockerClient", exception!.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CodeExecutionService(this.mockDockerClient.Object, null!, this.mockFileSystemService.Object));

        Assert.Equal("logger", exception!.ParamName);
    }

    [Fact]
    public void Constructor_WithNullFileSystemService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new CodeExecutionService(this.mockDockerClient.Object, this.mockLogger.Object, null!));

        Assert.Equal("fileSystemService", exception!.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Act
        var service = new CodeExecutionService(this.mockDockerClient.Object, this.mockLogger.Object, this.mockFileSystemService.Object);

        // Assert
        Assert.NotNull(service);
    }

    #endregion

    #region RunCodeAsync Tests

    [Fact]
    public async Task RunCodeAsync_WithNullUserCode_ThrowsArgumentNullException()
    {
        // Arrange
        var problem = CreateTestProblem();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.codeExecutionService.RunCodeAsync(null!, LanguageType.CSharp, problem, CancellationToken.None));
    }

    [Fact]
    public async Task RunCodeAsync_WithNullProblem_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await this.codeExecutionService.RunCodeAsync("test code", LanguageType.CSharp, null!, CancellationToken.None));
    }

    [Fact]
    public async Task RunCodeAsync_WithNoDriverForLanguage_ThrowsInvalidOperationException()
    {
        // Arrange
        var userCode = "test code";
        var problem = CreateTestProblem();
        problem.Drivers.Clear(); // Remove all drivers

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await this.codeExecutionService.RunCodeAsync(userCode, LanguageType.CSharp, problem, CancellationToken.None));

        Assert.Equal("No driver found for language: CSharp", exception!.Message);
    }

    [Fact]
    public async Task RunCodeAsync_WithValidInputAndSuccessfulExecution_ReturnsSuccessResult()
    {
        // Arrange
        var userCode = "test code";
        var problem = CreateTestProblem();
        var containerId = "test-container-id";

        var successResult = new SubmissionResult
        {
            Pass = true,
            TestCaseCount = 5,
            PassedTestCases = 5,
            FailedTestCases = 0,
            ErrorMessage = string.Empty,
            ExecutionTimeInMs = 150
        };

        this.SetupSuccessfulContainerExecution(containerId, successResult);

        // Act
        var result = await this.codeExecutionService.RunCodeAsync(userCode, LanguageType.CSharp, problem, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Pass);
        Assert.Equal(5, result.TestCaseCount);
        Assert.Equal(5, result.PassedTestCases);
        Assert.Equal(0, result.FailedTestCases);
        Assert.Empty(result.ErrorMessage);
        Assert.Equal(150, result.ExecutionTimeInMs);

        // Verify Docker interactions
        this.mockContainerOperations.Verify(x => x.CreateContainerAsync(
            It.IsAny<CreateContainerParameters>(),
            It.IsAny<CancellationToken>()), Times.Once);

        this.mockContainerOperations.Verify(x => x.StartContainerAsync(
            containerId,
            null,
            It.IsAny<CancellationToken>()), Times.Once);

        this.mockContainerOperations.Verify(x => x.WaitContainerAsync(
            containerId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunCodeAsync_WithValidInputAndFailedExecution_ReturnsFailureResult()
    {
        // Arrange
        var userCode = "test code";
        var problem = CreateTestProblem();
        var containerId = "test-container-id";

        var failureResult = new SubmissionResult
        {
            Pass = false,
            TestCaseCount = 5,
            PassedTestCases = 2,
            FailedTestCases = 3,
            ErrorMessage = "Test failed: Expected 10 but got 5",
            ExecutionTimeInMs = 200
        };

        this.SetupSuccessfulContainerExecution(containerId, failureResult);

        // Act
        var result = await this.codeExecutionService.RunCodeAsync(userCode, LanguageType.CSharp, problem, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Pass);
        Assert.Equal(5, result.TestCaseCount);
        Assert.Equal(2, result.PassedTestCases);
        Assert.Equal(3, result.FailedTestCases);
        Assert.Equal("Test failed: Expected 10 but got 5", result.ErrorMessage);
        Assert.Equal(200, result.ExecutionTimeInMs);
    }

    [Fact]
    public async Task RunCodeAsync_WithContainerExecutionError_ReturnsErrorResult()
    {
        // Arrange
        var userCode = "test code";
        var problem = CreateTestProblem();
        var containerId = "test-container-id";

        this.SetupContainerExecutionWithError(containerId, "Compilation error: Syntax error on line 5");

        // Act
        var result = await this.codeExecutionService.RunCodeAsync(userCode, LanguageType.CSharp, problem, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Pass);
        Assert.Equal(-1, result.TestCaseCount);
        Assert.Equal(-1, result.PassedTestCases);
        Assert.Equal(-1, result.FailedTestCases);
        Assert.Contains("Compilation error: Syntax error on line 5", result.ErrorMessage);
        Assert.Equal(-1, result.ExecutionTimeInMs);
    }

    [Fact]
    public async Task RunCodeAsync_WithDockerException_ReturnsErrorResult()
    {
        // Arrange
        var userCode = "test code";
        var problem = CreateTestProblem();

        this.mockContainerOperations
            .Setup(x => x.CreateContainerAsync(It.IsAny<CreateContainerParameters>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DockerApiException(System.Net.HttpStatusCode.InternalServerError, "Docker daemon error"));

        // Act
        var result = await this.codeExecutionService.RunCodeAsync(userCode, LanguageType.CSharp, problem, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Pass);
        Assert.Equal(-1, result.TestCaseCount);
        Assert.Equal(-1, result.PassedTestCases);
        Assert.Equal(-1, result.FailedTestCases);
        Assert.Contains("Docker daemon error", result.ErrorMessage);
        Assert.Equal(-1, result.ExecutionTimeInMs);

        // Verify error was logged
        this.VerifyLoggerWasCalled(LogLevel.Error, "Error executing code");
    }

    [Fact]
    public async Task RunCodeAsync_WithGenericException_ReturnsErrorResult()
    {
        // Arrange
        var userCode = "test code";
        var problem = CreateTestProblem();

        this.mockContainerOperations
            .Setup(x => x.CreateContainerAsync(It.IsAny<CreateContainerParameters>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await this.codeExecutionService.RunCodeAsync(userCode, LanguageType.CSharp, problem, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Pass);
        Assert.Equal(-1, result.TestCaseCount);
        Assert.Equal(-1, result.PassedTestCases);
        Assert.Equal(-1, result.FailedTestCases);
        Assert.Equal("Unexpected error", result.ErrorMessage);
        Assert.Equal(-1, result.ExecutionTimeInMs);

        // Verify error was logged
        this.VerifyLoggerWasCalled(LogLevel.Error, "Error executing code");
    }

    [Fact]
    public async Task RunCodeAsync_WithCancellationToken_PassesToDockerOperations()
    {
        // Arrange
        var userCode = "test code";
        var problem = CreateTestProblem();
        var cancellationToken = new CancellationToken(true);

        // Mock file system operations that will be called before Docker operations
        var tempDir = $@"temp\submission_{Guid.NewGuid()}";
        this.mockFileSystemService.Setup(x => x.GetTempPath()).Returns("temp");
        this.mockFileSystemService.Setup(x => x.CombinePaths("temp", It.IsAny<string>())).Returns(tempDir);
        this.mockFileSystemService.Setup(x => x.CreateDirectory(tempDir));

        // Set up ANY WriteAllTextAsync call with the canceled token to throw
        this.mockFileSystemService.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.Is<CancellationToken>(ct => ct.IsCancellationRequested)))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await this.codeExecutionService.RunCodeAsync(userCode, LanguageType.CSharp, problem, cancellationToken));
    }

    [Fact]
    public async Task RunCodeAsync_ValidatesContainerConfiguration()
    {
        // Arrange
        var userCode = "test code";
        var problem = CreateTestProblem();
        var containerId = "test-container-id";

        CreateContainerParameters? capturedParams = null;
        this.mockContainerOperations
            .Setup(x => x.CreateContainerAsync(It.IsAny<CreateContainerParameters>(), It.IsAny<CancellationToken>()))
            .Callback<CreateContainerParameters, CancellationToken>((p, ct) => capturedParams = p)
            .ReturnsAsync(new CreateContainerResponse { ID = containerId });

        this.mockContainerOperations
            .Setup(x => x.StartContainerAsync(containerId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this.mockContainerOperations
            .Setup(x => x.WaitContainerAsync(containerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContainerWaitResponse { StatusCode = 0 });

        // Setup file system mocks for successful execution
        var tempDir = $@"temp\submission_{Guid.NewGuid()}";
        this.mockFileSystemService.Setup(x => x.GetTempPath()).Returns("temp");
        this.mockFileSystemService.Setup(x => x.CombinePaths("temp", It.IsAny<string>())).Returns(tempDir);
        this.mockFileSystemService.Setup(x => x.CreateDirectory(tempDir));
        this.mockFileSystemService.Setup(x => x.DirectoryExists(tempDir)).Returns(true);
        this.mockFileSystemService.Setup(x => x.DeleteDirectory(tempDir, true));
        this.mockFileSystemService.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var resultsPath = $@"{tempDir}\results.json";
        var errorPath = $@"{tempDir}\error.txt";
        var outputPath = $@"{tempDir}\output.txt";

        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "results.json")).Returns(resultsPath);
        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "error.txt")).Returns(errorPath);
        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "output.txt")).Returns(outputPath);

        var successResult = new SubmissionResult { Pass = true };
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var resultJson = JsonSerializer.Serialize(successResult, jsonOptions);

        this.mockFileSystemService.Setup(x => x.FileExists(resultsPath)).Returns(true);
        this.mockFileSystemService.Setup(x => x.ReadAllTextAsync(resultsPath, It.IsAny<CancellationToken>())).ReturnsAsync(resultJson);
        this.mockFileSystemService.Setup(x => x.FileExists(errorPath)).Returns(false);
        this.mockFileSystemService.Setup(x => x.FileExists(outputPath)).Returns(false);

        // Act
        await this.codeExecutionService.RunCodeAsync(userCode, LanguageType.CSharp, problem, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedParams);
        Assert.Equal("test-csharp-image", capturedParams.Image);
        Assert.Equal(["/bin/bash", "/home/runner/run.sh"], capturedParams.Cmd);
        Assert.Equal("root", capturedParams.User);
        Assert.True(capturedParams.NetworkDisabled);

        // Verify host configuration
        Assert.NotNull(capturedParams.HostConfig);
        Assert.Equal(512L * 1024 * 1024, capturedParams.HostConfig.Memory); // 512MB
        Assert.Equal(512L * 1024 * 1024, capturedParams.HostConfig.MemorySwap); // 512MB
        Assert.Equal(50, capturedParams.HostConfig.CPUPercent);
        Assert.True(capturedParams.HostConfig.AutoRemove);
        Assert.Single(capturedParams.HostConfig.Binds);
        Assert.Contains(":/home/runner/submission:rw", capturedParams.HostConfig.Binds[0]);
    }

    [Fact]
    public async Task RunCodeAsync_WithMissingResultsFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var userCode = "test code";
        var problem = CreateTestProblem();
        var containerId = "test-container-id";

        this.SetupContainerExecutionWithoutResults(containerId);

        // Act
        var result = await this.codeExecutionService.RunCodeAsync(userCode, LanguageType.CSharp, problem, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Pass);
        Assert.Contains("Failed to deserialize results", result.ErrorMessage);
    }

    [Theory]
    [InlineData(LanguageType.CSharp)]
    [InlineData(LanguageType.JavaScript)]
    [InlineData(LanguageType.TypeScript)]
    public async Task RunCodeAsync_WorksWithAllSupportedLanguages(LanguageType language)
    {
        // Arrange
        var userCode = "test code";
        var problem = CreateTestProblem();
        problem.Drivers.Clear();
        problem.Drivers.Add(CreateTestDriver(language));

        var containerId = "test-container-id";
        var successResult = new SubmissionResult { Pass = true };

        this.SetupSuccessfulContainerExecution(containerId, successResult);

        // Act
        var result = await this.codeExecutionService.RunCodeAsync(userCode, language, problem, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Pass);
    }

    #endregion

    #region Helper Methods

    private static Problem CreateTestProblem()
    {
        return new Problem
        {
            Id = 1,
            Name = "Test Problem",
            Description = "A test problem",
            Difficulty = ProblemDifficulty.Easy,
            Input = [1, 2],
            ExpectedOutput = [3],
            Drivers = [CreateTestDriver(LanguageType.CSharp)]
        };
    }

    private static ProblemDriver CreateTestDriver(LanguageType language)
    {
        var imageName = language switch
        {
            LanguageType.CSharp => "test-csharp-image",
            LanguageType.JavaScript => "test-javascript-image",
            LanguageType.TypeScript => "test-typescript-image",
            _ => throw new ArgumentException($"Unsupported language: {language}")
        };

        return new ProblemDriver
        {
            Id = 1,
            Language = language,
            Image = imageName,
            DriverCode = "test driver code",
            UITemplate = "test ui template",
            Answer = "test answer"
        };
    }

    private void SetupContainerOperations(string containerId)
    {
        this.mockContainerOperations
            .Setup(x => x.CreateContainerAsync(It.IsAny<CreateContainerParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateContainerResponse { ID = containerId });

        this.mockContainerOperations
            .Setup(x => x.StartContainerAsync(containerId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this.mockContainerOperations
            .Setup(x => x.WaitContainerAsync(containerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ContainerWaitResponse { StatusCode = 0 });
    }

    private void SetupSuccessfulContainerExecution(string containerId, SubmissionResult result)
    {
        this.SetupContainerOperations(containerId);

        // Mock file system operations
        var tempDir = $@"temp\submission_{Guid.NewGuid()}";

        this.mockFileSystemService.Setup(x => x.GetTempPath()).Returns("temp");
        this.mockFileSystemService.Setup(x => x.CombinePaths("temp", It.IsAny<string>())).Returns(tempDir);
        this.mockFileSystemService.Setup(x => x.CreateDirectory(tempDir));
        this.mockFileSystemService.Setup(x => x.DirectoryExists(tempDir)).Returns(true);
        this.mockFileSystemService.Setup(x => x.DeleteDirectory(tempDir, true));
        this.mockFileSystemService.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Mock the results file content
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var resultJson = JsonSerializer.Serialize(result, jsonOptions);

        var resultsPath = $@"{tempDir}\results.json";
        var errorPath = $@"{tempDir}\error.txt";
        var outputPath = $@"{tempDir}\output.txt";

        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "results.json")).Returns(resultsPath);
        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "error.txt")).Returns(errorPath);
        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "output.txt")).Returns(outputPath);

        this.mockFileSystemService.Setup(x => x.FileExists(resultsPath)).Returns(true);
        this.mockFileSystemService.Setup(x => x.ReadAllTextAsync(resultsPath, It.IsAny<CancellationToken>())).ReturnsAsync(resultJson);
        this.mockFileSystemService.Setup(x => x.FileExists(errorPath)).Returns(false);
        this.mockFileSystemService.Setup(x => x.FileExists(outputPath)).Returns(false);
    }

    private void SetupContainerExecutionWithError(string containerId, string errorMessage)
    {
        this.SetupContainerOperations(containerId);

        // Mock file system operations
        var tempDir = $@"temp\submission_{Guid.NewGuid()}";

        this.mockFileSystemService.Setup(x => x.GetTempPath()).Returns("temp");
        this.mockFileSystemService.Setup(x => x.CombinePaths("temp", It.IsAny<string>())).Returns(tempDir);
        this.mockFileSystemService.Setup(x => x.CreateDirectory(tempDir));
        this.mockFileSystemService.Setup(x => x.DirectoryExists(tempDir)).Returns(true);
        this.mockFileSystemService.Setup(x => x.DeleteDirectory(tempDir, true));
        this.mockFileSystemService.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var resultsPath = $@"{tempDir}\results.json";
        var errorPath = $@"{tempDir}\error.txt";
        var outputPath = $@"{tempDir}\output.txt";

        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "results.json")).Returns(resultsPath);
        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "error.txt")).Returns(errorPath);
        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "output.txt")).Returns(outputPath);

        // Mock the error file content
        this.mockFileSystemService.Setup(x => x.FileExists(errorPath)).Returns(true);
        this.mockFileSystemService.Setup(x => x.ReadAllTextAsync(errorPath, It.IsAny<CancellationToken>())).ReturnsAsync(errorMessage);
        this.mockFileSystemService.Setup(x => x.FileExists(resultsPath)).Returns(false);
        this.mockFileSystemService.Setup(x => x.FileExists(outputPath)).Returns(false);
    }

    private void SetupContainerExecutionWithoutResults(string containerId)
    {
        this.SetupContainerOperations(containerId);

        // Mock file system operations without results.json
        var tempDir = $@"temp\submission_{Guid.NewGuid()}";

        this.mockFileSystemService.Setup(x => x.GetTempPath()).Returns("temp");
        this.mockFileSystemService.Setup(x => x.CombinePaths("temp", It.IsAny<string>())).Returns(tempDir);
        this.mockFileSystemService.Setup(x => x.CreateDirectory(tempDir));
        this.mockFileSystemService.Setup(x => x.DirectoryExists(tempDir)).Returns(true);
        this.mockFileSystemService.Setup(x => x.DeleteDirectory(tempDir, true));
        this.mockFileSystemService.Setup(x => x.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var resultsPath = $@"{tempDir}\results.json";
        var errorPath = $@"{tempDir}\error.txt";
        var outputPath = $@"{tempDir}\output.txt";

        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "results.json")).Returns(resultsPath);
        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "error.txt")).Returns(errorPath);
        this.mockFileSystemService.Setup(x => x.CombinePaths(tempDir, "output.txt")).Returns(outputPath);

        // Mock no results file
        this.mockFileSystemService.Setup(x => x.FileExists(resultsPath)).Returns(false);
        this.mockFileSystemService.Setup(x => x.FileExists(errorPath)).Returns(false);
        this.mockFileSystemService.Setup(x => x.FileExists(outputPath)).Returns(false);
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

    public void Dispose()
    {
        // No cleanup needed since we're using mocks
    }
}
