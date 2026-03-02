using System;

namespace QuestFlag.Passport.Application.Common.DTOs;

public record UserSummaryDto(
    Guid Id,
    Guid TenantId,
    string TenantName,
    string Username,
    string Email,
    string DisplayName,
    bool IsActive,
    bool TwoFactorEnabled,
    bool EmailConfirmed,
    List<string> Roles,
    List<string> AssignedAgentIds
);
