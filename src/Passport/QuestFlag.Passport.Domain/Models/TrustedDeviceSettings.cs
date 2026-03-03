namespace QuestFlag.Passport.Domain.Models;

public class TrustedDeviceSettings
{
    public const string SectionName = "TrustedDevices";

    public int ExpiryDays { get; set; }
}
