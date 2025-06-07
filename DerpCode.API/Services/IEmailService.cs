using System.Threading;
using System.Threading.Tasks;
using DerpCode.API.Models.Entities;

namespace DerpCode.API.Services;

public interface IEmailService
{
    Task SendEmailToUserAsync(User user, string subject, string plainTextMessage, string htmlMessage, CancellationToken cancellationToken);
}