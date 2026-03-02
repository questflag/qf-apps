namespace QuestFlag.Communication.Domain.Entities;

public class ProviderConfig
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string Credentials { get; set; } = string.Empty; // JSON
    public int Priority { get; set; }
}
