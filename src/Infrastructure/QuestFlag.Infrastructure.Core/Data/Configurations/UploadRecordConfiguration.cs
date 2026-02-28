using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestFlag.Infrastructure.Domain.Entities;

namespace QuestFlag.Infrastructure.Core.Data.Configurations;

public class UploadRecordConfiguration : IEntityTypeConfiguration<UploadRecord>
{
    public void Configure(EntityTypeBuilder<UploadRecord> builder)
    {
        builder.HasKey(x => x.Id);

        // Required fields
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.StoredFileName).IsRequired().HasMaxLength(500);
        builder.Property(x => x.BucketName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ObjectKey).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.TaskName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Category).IsRequired().HasMaxLength(50);
        
        // Map Tags array to PostgreSQL text[]
        builder.Property(x => x.Tags)
            .HasColumnType("text[]");

        // Map ExtraData dictionary to PostgreSQL jsonb
        builder.Property(x => x.ExtraData)
            .HasColumnType("jsonb");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Indexes for fast lookup
        builder.HasIndex(x => new { x.TenantId, x.UserId });
        builder.HasIndex(x => x.Status);
    }
}
