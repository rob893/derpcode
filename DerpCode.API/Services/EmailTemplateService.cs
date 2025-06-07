using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DerpCode.API.Services;

/// <summary>
/// Default implementation of the email template service.
/// </summary>
public sealed class EmailTemplateService : IEmailTemplateService
{
    private readonly IFileSystemService fileSystemService;

    private readonly string templatePath;

    private readonly ConcurrentDictionary<string, string> templateCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailTemplateService"/> class.
    /// </summary>
    public EmailTemplateService(IFileSystemService fileSystemService)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        this.templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates");
    }

    /// <inheritdoc />
    public async Task<(string PlainText, string Html)> GetEmailConfirmationTemplateAsync(string confirmationLink, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(confirmationLink))
        {
            throw new ArgumentException("Confirmation link cannot be null or empty.", nameof(confirmationLink));
        }

        var plainTextTemplate = await this.LoadTemplateAsync("EmailConfirmationTemplate.txt", cancellationToken);
        var htmlTemplate = await this.LoadTemplateAsync("EmailConfirmationTemplate.html", cancellationToken);

        var plainText = plainTextTemplate.Replace("{CONFIRMATION_LINK}", confirmationLink, StringComparison.Ordinal);
        var html = htmlTemplate.Replace("{CONFIRMATION_LINK}", confirmationLink, StringComparison.Ordinal);

        return (plainText, html);
    }

    /// <inheritdoc />
    public async Task<(string PlainText, string Html)> GetPasswordResetTemplateAsync(string resetLink, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resetLink))
        {
            throw new ArgumentException("Reset link cannot be null or empty.", nameof(resetLink));
        }

        var plainTextTemplate = await this.LoadTemplateAsync("PasswordResetTemplate.txt", cancellationToken);
        var htmlTemplate = await this.LoadTemplateAsync("PasswordResetTemplate.html", cancellationToken);

        var plainText = plainTextTemplate.Replace("{RESET_LINK}", resetLink, StringComparison.Ordinal);
        var html = htmlTemplate.Replace("{RESET_LINK}", resetLink, StringComparison.Ordinal);

        return (plainText, html);
    }

    private async Task<string> LoadTemplateAsync(string templateFileName, CancellationToken cancellationToken)
    {
        if (this.templateCache.TryGetValue(templateFileName, out var cachedTemplate))
        {
            return cachedTemplate;
        }

        var filePath = Path.Combine(this.templatePath, templateFileName);

        if (!this.fileSystemService.FileExists(filePath))
        {
            throw new FileNotFoundException($"Email template file not found: {filePath}");
        }

        var content = await this.fileSystemService.ReadAllTextAsync(filePath, cancellationToken);
        this.templateCache[templateFileName] = content;

        return content;
    }
}