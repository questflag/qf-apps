using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using QuestFlag.Passport.Domain.Contracts;
using QuestFlag.Passport.Domain.Models;

namespace QuestFlag.Passport.Core.Implementations.Messaging;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort);

        if (!string.IsNullOrEmpty(_settings.SmtpUser))
            client.Credentials = new NetworkCredential(_settings.SmtpUser, _settings.SmtpPassword);

        client.EnableSsl = _settings.SmtpPort != 25;

        var mail = new MailMessage(_settings.From, toEmail, subject, htmlBody) { IsBodyHtml = true };
        await client.SendMailAsync(mail, ct);
    }
}
