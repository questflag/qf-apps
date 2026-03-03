namespace QuestFlag.Passport.Domain.Contracts;

/// <summary>
/// Abstraction over an email sending provider (SMTP, SendGrid, etc.).
/// Implementations registered in Passport.Core.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}
