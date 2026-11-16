using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RepairTracker.Domain;

namespace RepairTracker.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Tenancy> Tenancies => Set<Tenancy>();
    public DbSet<Invitation> Invitations => Set<Invitation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity sets up its own tables first
        base.OnModelCreating(builder);

        builder.Entity<Property>(e =>
        {
            e.Property(p => p.AddressLine1).HasMaxLength(200);
            e.Property(p => p.AddressLine2).HasMaxLength(200);
            e.Property(p => p.Town).HasMaxLength(100);
            e.Property(p => p.Postcode).HasMaxLength(10);
            e.Property(p => p.Nickname).HasMaxLength(80);

            // Nearly every query is "this landlord's properties"
            e.HasIndex(p => p.LandlordId);

            // Can't delete a landlord who still has properties
            e.HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(p => p.LandlordId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Tenancy>(e =>
        {
            e.HasOne<Property>().WithMany().HasForeignKey(t => t.PropertyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<IdentityUser>().WithMany().HasForeignKey(t => t.TenantId).OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(t => t.PropertyId);

            // A tenant can only have one ongoing tenancy per property.
            // Ended ones don't count, so someone can move out and back in later.
            e.HasIndex(t => new { t.PropertyId, t.TenantId })
                .IsUnique()
                .HasFilter("[EndDate] IS NULL");
        });

        builder.Entity<Invitation>(e =>
        {
            e.Property(i => i.TokenHash).HasMaxLength(64);
            e.Property(i => i.Role).HasMaxLength(20);

            // Accepting an invite looks it up by hash, so this needs to be fast and unique
            e.HasIndex(i => i.TokenHash).IsUnique();

            e.HasOne<Property>().WithMany().HasForeignKey(i => i.PropertyId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<IdentityUser>().WithMany().HasForeignKey(i => i.CreatedById).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<IdentityUser>().WithMany().HasForeignKey(i => i.UsedById).OnDelete(DeleteBehavior.Restrict);
        });
    }
}