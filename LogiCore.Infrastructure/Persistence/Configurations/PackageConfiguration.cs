
using LogiCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        // Table name
        builder.ToTable("Packages");

        builder.HasKey(e => e.Id);

        // Unique index on TrackingNumber
        builder.HasIndex(e => e.TrackingNumber)
            .IsUnique();

        builder.Property(e => e.TrackingNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.RecipientName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Weight)
            .HasColumnType("decimal(10,2)") 
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.ApplicationUserId)
            .HasMaxLength(450);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}