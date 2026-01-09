using System;
using Azure.Communication.Email;
using Azure.Core;
using Azure.Identity;
using DerpCode.API.Models.Settings;
using Microsoft.Extensions.Options;

namespace DerpCode.API.Utilities;

public sealed class AcsEmailClientFactory : IAcsEmailClientFactory
{
    private readonly EmailSettings emailSettings;

    public AcsEmailClientFactory(IOptions<EmailSettings> emailSettings)
    {
        this.emailSettings = emailSettings?.Value ?? throw new ArgumentNullException(nameof(emailSettings));
    }

    public EmailClient CreateClient(TokenCredential? tokenCredential = null)
    {
        return new EmailClient(this.emailSettings.AcsEndpoint, tokenCredential ?? new DefaultAzureCredential());
    }
}