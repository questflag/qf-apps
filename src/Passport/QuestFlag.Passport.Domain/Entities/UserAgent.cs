using System;

namespace QuestFlag.Passport.Domain.Entities;

public class UserAgent
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string ClientId { get; set; } = string.Empty;
}
