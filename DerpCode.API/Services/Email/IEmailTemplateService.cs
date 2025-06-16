using System.Threading;
using System.Threading.Tasks;

namespace DerpCode.API.Services.Email;

/// <summary>
/// Service for loading and processing email templates.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Gets the email confirmation template content.
    /// </summary>
    /// <param name="confirmationLink">The confirmation link to include in the template.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A tuple containing the plain text and HTML versions of the email.</returns>
    Task<(string PlainText, string Html)> GetEmailConfirmationTemplateAsync(string confirmationLink, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the password reset template content.
    /// </summary>
    /// <param name="resetLink">The password reset link to include in the template.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A tuple containing the plain text and HTML versions of the email.</returns>
    Task<(string PlainText, string Html)> GetPasswordResetTemplateAsync(string resetLink, CancellationToken cancellationToken);
}