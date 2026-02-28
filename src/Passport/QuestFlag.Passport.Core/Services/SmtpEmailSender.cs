using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Core.Services;

/// <summary>
/// SMTP-based email sender. Configuration keys:
///   Email:SmtpHost, Email:SmtpPort (default 587), Email:SmtpUser, Email:SmtpPassword, Email:From
/// For development, set SmtpHost to "localhost" and use MailHog or similar.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string? _user;
    private readonly string? _password;
    private readonly string _from;

    public SmtpEmailSender(IConfiguration config)
    {
        _host = config["Email:SmtpHost"] ?? "localhost";
        _port = int.TryParse(config["Email:SmtpPort"], out var p) ? p : 587;
        _user = config["Email:SmtpUser"];
        _password = config["Email:SmtpPassword"];
        _from = config["Email:From"] ?? "noreply@questflag.com";
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        using var client = new SmtpClient(_host, _port);

        if (!string.IsNullOrEmpty(_user))
            client.Credentials = new NetworkCredential(_user, _password);

        client.EnableSsl = _port != 25;

        var mail = new MailMessage(_from, toEmail, subject, htmlBody) { IsBodyHtml = true };
        await client.SendMailAsync(mail, ct);
    }
}
