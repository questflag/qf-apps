using System.Threading;
using System.Threading.Tasks;

namespace QuestFlag.Passport.Domain.Interfaces;

/// <summary>
/// Abstraction over an email sending provider (SMTP, SendGrid, etc.).
/// Implementations registered in Passport.Core.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}
