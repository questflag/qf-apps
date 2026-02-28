using System;

namespace QuestFlag.Passport.Application.Common.DTOs;

public record UserSummaryDto(
    Guid Id,
    string Username,
    string Email,
    string DisplayName,
    bool IsActive,
    string Role
);
