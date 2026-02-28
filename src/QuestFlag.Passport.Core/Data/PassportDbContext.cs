using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuestFlag.Passport.Domain.Entities;
using System;

namespace QuestFlag.Passport.Core.Data;

public class PassportDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public PassportDbContext(DbContextOptions<PassportDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration from the current assembly
        builder.ApplyConfigurationsFromAssembly(typeof(PassportDbContext).Assembly);
    }
}
