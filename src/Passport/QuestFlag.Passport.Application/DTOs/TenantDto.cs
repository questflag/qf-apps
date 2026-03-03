namespace QuestFlag.Passport.Application.DTOs;

public record TenantDto(Guid Id, string Name, string Slug, bool IsActive, DateTime CreatedAtUtc);
