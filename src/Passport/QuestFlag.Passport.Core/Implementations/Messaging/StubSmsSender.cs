using Microsoft.Extensions.Logging;
using QuestFlag.Passport.Domain.Contracts;

namespace QuestFlag.Passport.Core.Implementations.Messaging;

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
