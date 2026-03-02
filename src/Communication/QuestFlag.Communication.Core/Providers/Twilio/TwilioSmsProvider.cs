using QuestFlag.Communication.Domain.Entities;

namespace QuestFlag.Communication.Core.Providers.Twilio;

public class TwilioSmsProvider
{
    public async Task<bool> SendSmsAsync(string recipient, string message, string accountSid, string authToken)
    {
        // Placeholder for Twilio SDK call
        return await Task.FromResult(true);
    }
}
