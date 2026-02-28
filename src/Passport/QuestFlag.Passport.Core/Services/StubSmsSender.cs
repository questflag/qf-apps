using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestFlag.Passport.Domain.Interfaces;

namespace QuestFlag.Passport.Core.Services;

/// <summary>
/// Development stub SMS sender — logs OTP to console/logger instead of sending a real SMS.
/// Replace with TwilioSmsSender or AwsSnsSmsSender for production.
/// Configuration key: Sms:Provider ("stub" | "twilio" | "sns")
/// </summary>
public class StubSmsSender : ISmsSender
{
    private readonly ILogger<StubSmsSender> _logger;

    public StubSmsSender(ILogger<StubSmsSender> logger) => _logger = logger;

    public Task SendAsync(string toPhoneNumber, string messageBody, CancellationToken ct = default)
    {
        _logger.LogWarning("[SMS STUB] To: {Phone} | Message: {Body}", toPhoneNumber, messageBody);
        Console.WriteLine($"[SMS STUB] >>> To: {toPhoneNumber} | {messageBody}");
        return Task.CompletedTask;
    }
}
