namespace QuestFlag.Passport.Services.Models;

public class PassportDbSettings
{
    public bool RunMigrationsOnStartup { get; set; } = true;
    public bool SeedDefaultData { get; set; } = true;
}
