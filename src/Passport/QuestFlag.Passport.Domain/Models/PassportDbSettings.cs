namespace QuestFlag.Passport.Domain.Models;

public class PassportDbSettings
{
    public const string SectionName = "PassportDbSettings";

    public bool RunMigrationsOnStartup { get; set; }
    public bool SeedDefaultData { get; set; }
}
