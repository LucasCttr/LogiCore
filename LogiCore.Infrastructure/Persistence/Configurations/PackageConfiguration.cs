
using LogiCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TrackingNumber).IsRequired().HasMaxLength(50);
        builder.Property(e => e.RecipientName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Weight).HasPrecision(10, 2);
    }
}