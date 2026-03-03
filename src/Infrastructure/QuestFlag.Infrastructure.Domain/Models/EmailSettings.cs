namespace QuestFlag.Infrastructure.Domain.Models;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string From { get; set; } = string.Empty;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
}
