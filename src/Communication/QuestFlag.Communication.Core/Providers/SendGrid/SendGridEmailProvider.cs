namespace QuestFlag.Communication.Core.Providers.SendGrid;

public class SendGridEmailProvider
{
    public async Task<bool> SendEmailAsync(string recipient, string subject, string body, string apiKey)
    {
        // Placeholder for SendGrid SDK call
        return await Task.FromResult(true);
    }
}
