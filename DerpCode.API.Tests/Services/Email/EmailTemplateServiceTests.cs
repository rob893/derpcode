using DerpCode.API.Services.Core;
using DerpCode.API.Services.Email;

namespace DerpCode.API.Tests.Services.Email;

/// <summary>
/// Unit tests for the <see cref="EmailTemplateService"/> class.
/// </summary>
public sealed class EmailTemplateServiceTests
{
    private readonly Mock<IFileSystemService> mockFileSystemService;

    private readonly EmailTemplateService emailTemplateService;

    public EmailTemplateServiceTests()
    {
        this.mockFileSystemService = new Mock<IFileSystemService>();
        this.emailTemplateService = new EmailTemplateService(this.mockFileSystemService.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidFileSystemService_ShouldCreateInstance()
    {
        // Act
        var service = new EmailTemplateService(this.mockFileSystemService.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullFileSystemService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new EmailTemplateService(null!));

        Assert.Equal("fileSystemService", exception.ParamName);
    }

    #endregion

    #region GetEmailConfirmationTemplateAsync Tests

    [Fact]
    public async Task GetEmailConfirmationTemplateAsync_WithValidConfirmationLink_ShouldReturnPopulatedTemplates()
    {
        // Arrange
        var confirmationLink = "https://example.com/confirm?token=abc123";
        var plainTextTemplate = "Please confirm your email by clicking this link: {CONFIRMATION_LINK}";
        var htmlTemplate = "<html><body>Please confirm your email by clicking <a href=\"{CONFIRMATION_LINK}\">here</a></body></html>";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("EmailConfirmationTemplate.txt", plainTextTemplate);
        this.SetupFileSystemMocks("EmailConfirmationTemplate.html", htmlTemplate);

        // Act
        var result = await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink, cancellationToken);

        // Assert
        Assert.Equal("Please confirm your email by clicking this link: https://example.com/confirm?token=abc123", result.PlainText);
        Assert.Equal("<html><body>Please confirm your email by clicking <a href=\"https://example.com/confirm?token=abc123\">here</a></body></html>", result.Html);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetEmailConfirmationTemplateAsync_WithNullOrWhitespaceConfirmationLink_ShouldThrowArgumentException(string? confirmationLink)
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink!, cancellationToken));

        Assert.Equal("confirmationLink", exception.ParamName);
        Assert.Contains("Confirmation link cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task GetEmailConfirmationTemplateAsync_WithMissingPlainTextTemplate_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var confirmationLink = "https://example.com/confirm?token=abc123";
        var cancellationToken = CancellationToken.None;

        this.mockFileSystemService.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(
            () => this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink, cancellationToken));

        Assert.Contains("Email template file not found", exception.Message);
        Assert.Contains("EmailConfirmationTemplate.txt", exception.Message);
    }

    [Fact]
    public async Task GetEmailConfirmationTemplateAsync_WithSpecialCharactersInLink_ShouldHandleCorrectly()
    {
        // Arrange
        var confirmationLink = "https://example.com/confirm?token=abc123&email=test%40example.com&redirect=%2Fhome";
        var plainTextTemplate = "Link: {CONFIRMATION_LINK}";
        var htmlTemplate = "<a href=\"{CONFIRMATION_LINK}\">Confirm</a>";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("EmailConfirmationTemplate.txt", plainTextTemplate);
        this.SetupFileSystemMocks("EmailConfirmationTemplate.html", htmlTemplate);

        // Act
        var result = await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink, cancellationToken);

        // Assert
        Assert.Contains(confirmationLink, result.PlainText);
        Assert.Contains(confirmationLink, result.Html);
    }

    [Fact]
    public async Task GetEmailConfirmationTemplateAsync_CalledTwiceWithSameData_ShouldUseCachedTemplates()
    {
        // Arrange
        var confirmationLink1 = "https://example.com/confirm?token=abc123";
        var confirmationLink2 = "https://example.com/confirm?token=def456";
        var plainTextTemplate = "Please confirm: {CONFIRMATION_LINK}";
        var htmlTemplate = "<a href=\"{CONFIRMATION_LINK}\">Confirm</a>";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("EmailConfirmationTemplate.txt", plainTextTemplate);
        this.SetupFileSystemMocks("EmailConfirmationTemplate.html", htmlTemplate);

        // Act
        var result1 = await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink1, cancellationToken);
        var result2 = await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink2, cancellationToken);

        // Assert
        Assert.Contains(confirmationLink1, result1.PlainText);
        Assert.Contains(confirmationLink2, result2.PlainText);

        // Verify file system was only called once per template (caching works)
        this.mockFileSystemService.Verify(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion

    #region GetPasswordResetTemplateAsync Tests

    [Fact]
    public async Task GetPasswordResetTemplateAsync_WithValidResetLink_ShouldReturnPopulatedTemplates()
    {
        // Arrange
        var resetLink = "https://example.com/reset?token=xyz789";
        var plainTextTemplate = "Reset your password by clicking this link: {RESET_LINK}";
        var htmlTemplate = "<html><body>Reset your password by clicking <a href=\"{RESET_LINK}\">here</a></body></html>";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("PasswordResetTemplate.txt", plainTextTemplate);
        this.SetupFileSystemMocks("PasswordResetTemplate.html", htmlTemplate);

        // Act
        var result = await this.emailTemplateService.GetPasswordResetTemplateAsync(resetLink, cancellationToken);

        // Assert
        Assert.Equal("Reset your password by clicking this link: https://example.com/reset?token=xyz789", result.PlainText);
        Assert.Equal("<html><body>Reset your password by clicking <a href=\"https://example.com/reset?token=xyz789\">here</a></body></html>", result.Html);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPasswordResetTemplateAsync_WithNullOrWhitespaceResetLink_ShouldThrowArgumentException(string? resetLink)
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => this.emailTemplateService.GetPasswordResetTemplateAsync(resetLink!, cancellationToken));

        Assert.Equal("resetLink", exception.ParamName);
        Assert.Contains("Reset link cannot be null or empty", exception.Message);
    }

    [Fact]
    public async Task GetPasswordResetTemplateAsync_WithMissingHtmlTemplate_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var resetLink = "https://example.com/reset?token=xyz789";
        var plainTextTemplate = "Reset your password: {RESET_LINK}";
        var cancellationToken = CancellationToken.None;

        // Setup plain text template to exist, but HTML template to not exist
        this.mockFileSystemService.Setup(x => x.FileExists(It.Is<string>(path => path.Contains("PasswordResetTemplate.txt")))).Returns(true);
        this.mockFileSystemService.Setup(x => x.FileExists(It.Is<string>(path => path.Contains("PasswordResetTemplate.html")))).Returns(false);
        this.mockFileSystemService.Setup(x => x.ReadAllTextAsync(It.Is<string>(path => path.Contains("PasswordResetTemplate.txt")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plainTextTemplate);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(
            () => this.emailTemplateService.GetPasswordResetTemplateAsync(resetLink, cancellationToken));

        Assert.Contains("Email template file not found", exception.Message);
        Assert.Contains("PasswordResetTemplate.html", exception.Message);
    }

    [Fact]
    public async Task GetPasswordResetTemplateAsync_WithLongResetLink_ShouldHandleCorrectly()
    {
        // Arrange
        var resetLink = "https://example.com/reset?token=" + new string('a', 1000) + "&user=12345&expires=1640995200";
        var plainTextTemplate = "Reset link: {RESET_LINK}";
        var htmlTemplate = "<a href=\"{RESET_LINK}\">Reset Password</a>";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("PasswordResetTemplate.txt", plainTextTemplate);
        this.SetupFileSystemMocks("PasswordResetTemplate.html", htmlTemplate);

        // Act
        var result = await this.emailTemplateService.GetPasswordResetTemplateAsync(resetLink, cancellationToken);

        // Assert
        Assert.Contains(resetLink, result.PlainText);
        Assert.Contains(resetLink, result.Html);
        Assert.True(result.PlainText.Length > 1000);
        Assert.True(result.Html.Length > 1000);
    }

    [Fact]
    public async Task GetPasswordResetTemplateAsync_CalledTwiceWithSameData_ShouldUseCachedTemplates()
    {
        // Arrange
        var resetLink1 = "https://example.com/reset?token=xyz789";
        var resetLink2 = "https://example.com/reset?token=uvw456";
        var plainTextTemplate = "Reset password: {RESET_LINK}";
        var htmlTemplate = "<a href=\"{RESET_LINK}\">Reset</a>";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("PasswordResetTemplate.txt", plainTextTemplate);
        this.SetupFileSystemMocks("PasswordResetTemplate.html", htmlTemplate);

        // Act
        var result1 = await this.emailTemplateService.GetPasswordResetTemplateAsync(resetLink1, cancellationToken);
        var result2 = await this.emailTemplateService.GetPasswordResetTemplateAsync(resetLink2, cancellationToken);

        // Assert
        Assert.Contains(resetLink1, result1.PlainText);
        Assert.Contains(resetLink2, result2.PlainText);

        // Verify file system was only called once per template (caching works)
        this.mockFileSystemService.Verify(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion

    #region Template Caching Tests

    [Fact]
    public async Task MultipleTemplateMethods_ShouldShareCacheCorrectly()
    {
        // Arrange
        var confirmationLink = "https://example.com/confirm?token=abc123";
        var resetLink = "https://example.com/reset?token=xyz789";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("EmailConfirmationTemplate.txt", "Confirm: {CONFIRMATION_LINK}");
        this.SetupFileSystemMocks("EmailConfirmationTemplate.html", "<a href=\"{CONFIRMATION_LINK}\">Confirm</a>");
        this.SetupFileSystemMocks("PasswordResetTemplate.txt", "Reset: {RESET_LINK}");
        this.SetupFileSystemMocks("PasswordResetTemplate.html", "<a href=\"{RESET_LINK}\">Reset</a>");

        // Act
        await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink, cancellationToken);
        await this.emailTemplateService.GetPasswordResetTemplateAsync(resetLink, cancellationToken);
        await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink, cancellationToken); // Should use cache
        await this.emailTemplateService.GetPasswordResetTemplateAsync(resetLink, cancellationToken); // Should use cache

        // Assert
        // Verify each template file was only read once (4 total: 2 confirmation + 2 reset)
        this.mockFileSystemService.Verify(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(4));
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task GetEmailConfirmationTemplateAsync_WithCancelledToken_ShouldRespectCancellation()
    {
        // Arrange
        var confirmationLink = "https://example.com/confirm?token=abc123";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        this.SetupFileSystemMocks("EmailConfirmationTemplate.txt", "Confirm: {CONFIRMATION_LINK}");
        this.mockFileSystemService.Setup(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink, cts.Token));
    }

    [Fact]
    public async Task GetPasswordResetTemplateAsync_WithCancelledToken_ShouldRespectCancellation()
    {
        // Arrange
        var resetLink = "https://example.com/reset?token=xyz789";
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        this.SetupFileSystemMocks("PasswordResetTemplate.txt", "Reset: {RESET_LINK}");
        this.mockFileSystemService.Setup(x => x.ReadAllTextAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => this.emailTemplateService.GetPasswordResetTemplateAsync(resetLink, cts.Token));
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task GetEmailConfirmationTemplateAsync_WithMultipleReplacementTokens_ShouldReplaceAllOccurrences()
    {
        // Arrange
        var confirmationLink = "https://example.com/confirm?token=abc123";
        var plainTextTemplate = "Link: {CONFIRMATION_LINK} - Please visit {CONFIRMATION_LINK} to confirm.";
        var htmlTemplate = "<p>Link: {CONFIRMATION_LINK}</p><p>Visit {CONFIRMATION_LINK} again</p>";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("EmailConfirmationTemplate.txt", plainTextTemplate);
        this.SetupFileSystemMocks("EmailConfirmationTemplate.html", htmlTemplate);

        // Act
        var result = await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink, cancellationToken);

        // Assert
        Assert.Equal("Link: https://example.com/confirm?token=abc123 - Please visit https://example.com/confirm?token=abc123 to confirm.", result.PlainText);
        Assert.Equal("<p>Link: https://example.com/confirm?token=abc123</p><p>Visit https://example.com/confirm?token=abc123 again</p>", result.Html);
    }

    [Fact]
    public async Task GetPasswordResetTemplateAsync_WithMultipleReplacementTokens_ShouldReplaceAllOccurrences()
    {
        // Arrange
        var resetLink = "https://example.com/reset?token=xyz789";
        var plainTextTemplate = "Link: {RESET_LINK} - Please visit {RESET_LINK} to reset.";
        var htmlTemplate = "<p>Link: {RESET_LINK}</p><p>Visit {RESET_LINK} again</p>";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("PasswordResetTemplate.txt", plainTextTemplate);
        this.SetupFileSystemMocks("PasswordResetTemplate.html", htmlTemplate);

        // Act
        var result = await this.emailTemplateService.GetPasswordResetTemplateAsync(resetLink, cancellationToken);

        // Assert
        Assert.Equal("Link: https://example.com/reset?token=xyz789 - Please visit https://example.com/reset?token=xyz789 to reset.", result.PlainText);
        Assert.Equal("<p>Link: https://example.com/reset?token=xyz789</p><p>Visit https://example.com/reset?token=xyz789 again</p>", result.Html);
    }

    [Fact]
    public async Task EmailTemplateService_WithEmptyTemplateFiles_ShouldReturnOnlyLink()
    {
        // Arrange
        var confirmationLink = "https://example.com/confirm?token=abc123";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("EmailConfirmationTemplate.txt", "{CONFIRMATION_LINK}");
        this.SetupFileSystemMocks("EmailConfirmationTemplate.html", "{CONFIRMATION_LINK}");

        // Act
        var result = await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink, cancellationToken);

        // Assert
        Assert.Equal(confirmationLink, result.PlainText);
        Assert.Equal(confirmationLink, result.Html);
    }

    [Fact]
    public async Task EmailTemplateService_WithNoReplacementTokens_ShouldReturnTemplateAsIs()
    {
        // Arrange
        var confirmationLink = "https://example.com/confirm?token=abc123";
        var plainTextTemplate = "This template has no replacement tokens.";
        var htmlTemplate = "<html><body>This template has no replacement tokens.</body></html>";
        var cancellationToken = CancellationToken.None;

        this.SetupFileSystemMocks("EmailConfirmationTemplate.txt", plainTextTemplate);
        this.SetupFileSystemMocks("EmailConfirmationTemplate.html", htmlTemplate);

        // Act
        var result = await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confirmationLink, cancellationToken);

        // Assert
        Assert.Equal(plainTextTemplate, result.PlainText);
        Assert.Equal(htmlTemplate, result.Html);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sets up file system mocks for a specific template file.
    /// </summary>
    /// <param name="fileName">The template file name.</param>
    /// <param name="content">The template content to return.</param>
    private void SetupFileSystemMocks(string fileName, string content)
    {
        this.mockFileSystemService.Setup(x => x.FileExists(It.Is<string>(path => path.Contains(fileName))))
            .Returns(true);
        this.mockFileSystemService.Setup(x => x.ReadAllTextAsync(It.Is<string>(path => path.Contains(fileName)), It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);
    }

    #endregion
}