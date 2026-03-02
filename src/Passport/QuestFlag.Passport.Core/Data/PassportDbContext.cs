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
    public DbSet<TrustedDevice> TrustedDevices => Set<TrustedDevice>();
    public DbSet<UserAgent> UserAgents => Set<UserAgent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Map OpenIddict entities
        builder.UseOpenIddict();

        builder.Entity<UserAgent>()
            .HasKey(ua => new { ua.UserId, ua.ClientId });

        builder.Entity<UserAgent>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.UserAgents)
            .HasForeignKey(ua => ua.UserId);

        // Apply all IEntityTypeConfiguration from the current assembly
        builder.ApplyConfigurationsFromAssembly(typeof(PassportDbContext).Assembly);
    }
}
