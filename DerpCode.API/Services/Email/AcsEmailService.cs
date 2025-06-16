using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;
using DerpCode.API.Models.Entities;
using DerpCode.API.Models.Settings;
using DerpCode.API.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DerpCode.API.Services.Email;

public sealed class AcsEmailService : IEmailService
{
    private readonly EmailSettings emailSettings;

    private readonly IAcsEmailClientFactory emailClientFactory;

    private readonly IEmailTemplateService emailTemplateService;

    private readonly AuthenticationSettings authSettings;

    private readonly ILogger<AcsEmailService> logger;

    public AcsEmailService(
        IOptions<EmailSettings> emailSettings,
        IAcsEmailClientFactory emailClientFactory,
        IEmailTemplateService emailTemplateService,
        IOptions<AuthenticationSettings> authSettings,
        ILogger<AcsEmailService> logger)
    {
        this.emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
        this.emailClientFactory = emailClientFactory ?? throw new ArgumentNullException(nameof(emailClientFactory));
        this.emailTemplateService = emailTemplateService ?? throw new ArgumentNullException(nameof(emailTemplateService));
        this.authSettings = authSettings?.Value ?? throw new ArgumentNullException(nameof(authSettings));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendEmailConfirmationToUserAsync(User user, string token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(user.Email);

        var confLink = $"{this.authSettings.UIBaseUrl}#/confirm-email?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
        var (plainTextMessage, htmlMessage) = await this.emailTemplateService.GetEmailConfirmationTemplateAsync(confLink, cancellationToken);
        await this.SendEmailToUserAsync(user, "DerpCode Email Confirmation - Verify Your Account! 📧", plainTextMessage, htmlMessage, cancellationToken);
    }

    public async Task SendResetPasswordToUserAsync(User user, string token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(token);
        ArgumentException.ThrowIfNullOrEmpty(user.Email);

        var confLink = $"{this.authSettings.UIBaseUrl}#/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
        var (plainTextMessage, htmlMessage) = await this.emailTemplateService.GetPasswordResetTemplateAsync(confLink, cancellationToken);
        await this.SendEmailToUserAsync(user, "DerpCode Password Reset - Let's Get You Back In! 🔑", plainTextMessage, htmlMessage, cancellationToken);
    }

    public async Task SendEmailToUserAsync(User user, string subject, string plainTextMessage, string htmlMessage, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(plainTextMessage);
        ArgumentException.ThrowIfNullOrEmpty(htmlMessage);

        if (!this.emailSettings.Enabled)
        {
            this.logger.LogWarning("Email service is disabled. Not sending email to user {UserId} with subject {Subject}", user.Id, subject);
            return;
        }

        var emailClient = this.emailClientFactory.CreateClient();

        var emailMessage = new EmailMessage(
            senderAddress: this.emailSettings.FromAddress,
            content: new EmailContent(subject)
            {
                PlainText = plainTextMessage,
                Html = htmlMessage
            },
            recipients: new EmailRecipients([new EmailAddress(user.Email)]));

        try
        {
            this.logger.LogDebug("Sending email to user {UserId} with subject {Subject}", user.Id, subject);
            await emailClient.SendAsync(WaitUntil.Started, emailMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error when sending email to {UserId} with subject {Subject}", user.Id, subject);
            throw;
        }
    }
}