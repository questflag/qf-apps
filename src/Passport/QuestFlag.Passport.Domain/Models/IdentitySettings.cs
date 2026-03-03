namespace QuestFlag.Passport.Domain.Models;

/// <summary>
/// Strongly-typed binding for the "Identity" configuration section.
/// Controls ASP.NET Core Identity password policy and user requirements.
/// </summary>
public class IdentitySettings
{
    public const string SectionName = "Identity";

    // Password policy
    public bool RequireDigit { get; set; } = false;
    public int RequiredLength { get; set; } = 6;
    public bool RequireNonAlphanumeric { get; set; } = false;
    public bool RequireUppercase { get; set; } = false;
    public bool RequireLowercase { get; set; } = false;

    // User policy
    public bool RequireUniqueEmail { get; set; } = false;

    // Sign-in policy
    public bool RequireConfirmedEmail { get; set; } = false;
}
