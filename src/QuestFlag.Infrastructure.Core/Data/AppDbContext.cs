using Microsoft.EntityFrameworkCore;
using QuestFlag.Infrastructure.Domain.Entities;

namespace QuestFlag.Infrastructure.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UploadRecord> UploadRecords => Set<UploadRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
