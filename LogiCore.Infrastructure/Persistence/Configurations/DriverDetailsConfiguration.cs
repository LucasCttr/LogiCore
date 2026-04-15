using LogiCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiCore.Infrastructure.Persistence.Configurations;

public class DriverDetailsConfiguration : IEntityTypeConfiguration<DriverDetails>
{
    public void Configure(EntityTypeBuilder<DriverDetails> builder)
    {
        builder.ToTable("DriverDetails");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(d => d.LicenseType)
            .IsRequired()
            .HasMaxLength(10); // A, B, C, D, etc.

        builder.Property(d => d.LicenseExpiry)
            .IsRequired();

        builder.Property(d => d.InsuranceExpiry)
            .IsRequired();

        builder.Property(d => d.AssignedVehicleId);

        builder.Property(d => d.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(d => d.UpdatedAt);

        // Configure relationship with ApplicationUser (1:1)
        builder.HasOne(d => d.User)
            .WithOne(u => u.DriverDetails)
            .HasForeignKey<DriverDetails>(d => d.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Configure relationship with Vehicle
        builder.HasOne(d => d.AssignedVehicle)
            .WithMany()
            .HasForeignKey(d => d.AssignedVehicleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Add index for UserId for quick lookups
        builder.HasIndex(d => d.UserId).IsUnique();
    }
}
