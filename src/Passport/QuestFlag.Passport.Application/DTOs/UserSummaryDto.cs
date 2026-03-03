namespace QuestFlag.Passport.Application.DTOs;

public record UserSummaryDto(
    Guid Id,
    Guid TenantId,
    string TenantName,
    string UserName,
    string Email,
    string? FullName,
    bool IsActive,
    bool TwoFactorEnabled,
    bool EmailConfirmed,
    List<string> Roles,
    HashSet<string> AssignedAgentIds);
