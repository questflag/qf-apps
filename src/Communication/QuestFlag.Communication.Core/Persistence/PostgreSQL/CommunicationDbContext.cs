using Microsoft.EntityFrameworkCore;
using QuestFlag.Communication.Domain.Entities;

namespace QuestFlag.Communication.Core.Persistence.PostgreSQL;

public class CommunicationDbContext : DbContext
{
    public CommunicationDbContext(DbContextOptions<CommunicationDbContext> options) : base(options) { }

    public DbSet<ProviderConfig> ProviderConfigs => Set<ProviderConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProviderConfig>(entity =>
        {
            entity.ToTable("PROVIDER_CONFIG");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TenantId).IsRequired();
            entity.Property(e => e.ProviderType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Credentials).IsRequired();
            entity.Property(e => e.Priority).IsDefaultValue(0);
        });
    }
}
