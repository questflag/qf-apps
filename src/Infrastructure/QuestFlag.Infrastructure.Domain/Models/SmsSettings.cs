namespace QuestFlag.Infrastructure.Domain.Models;

public class SmsSettings
{
    public const string SectionName = "Sms";

    public string Provider { get; set; } = string.Empty;
}
