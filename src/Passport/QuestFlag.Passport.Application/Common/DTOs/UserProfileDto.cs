using System;

namespace QuestFlag.Passport.Application.Common.DTOs;

public record UserProfileDto(
    Guid Id,
    string Email,
    string Name,
    bool EmailConfirmed,
    string? PhoneNumber,
    bool TwoFactorEnabled
);
