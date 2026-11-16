using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RepairTracker.Domain;

namespace RepairTracker.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();

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
    }
}