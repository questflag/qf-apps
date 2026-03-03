namespace QuestFlag.Passport.Application.DTOs;

public record UserProfileDto(
    Guid Id,
    string Email,
    string? FullName,
    bool EmailConfirmed,
    string? PhoneNumber,
    bool TwoFactorEnabled);
