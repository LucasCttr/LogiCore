
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

        builder.OwnsOne(p => p.Recipient, r =>
        {
            r.Property(x => x.Name).HasColumnName("RecipientName").IsRequired().HasMaxLength(200);
            r.Property(x => x.Address).HasColumnName("RecipientAddress").HasMaxLength(500);
            r.Property(x => x.Phone).HasColumnName("RecipientPhone").HasMaxLength(50);
        });

        builder.Property(e => e.Weight)
            .HasColumnType("decimal(10,2)") 
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.ApplicationUserId)
            .HasMaxLength(450);

        builder.HasOne(p => p.ApplicationUser)
            .WithMany(u => u.Packages)
            .HasForeignKey(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}