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

namespace DerpCode.API.Services;

public sealed class AcsEmailService : IEmailService
{
    private readonly EmailSettings emailSettings;

    private readonly IAcsEmailClientFactory emailClientFactory;

    private readonly ILogger<AcsEmailService> logger;

    public AcsEmailService(IOptions<EmailSettings> emailSettings, IAcsEmailClientFactory emailClientFactory, ILogger<AcsEmailService> logger)
    {
        this.emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
        this.emailClientFactory = emailClientFactory ?? throw new ArgumentNullException(nameof(emailClientFactory));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SendEmailToUserAsync(User user, string subject, string plainTextMessage, string htmlMessage, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(plainTextMessage);
        ArgumentException.ThrowIfNullOrEmpty(htmlMessage);

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