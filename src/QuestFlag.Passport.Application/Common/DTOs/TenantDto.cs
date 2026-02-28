using System;

namespace QuestFlag.Passport.Application.Common.DTOs;

public record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    DateTime CreatedAtUtc
);
