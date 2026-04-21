using LogiCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogiCore.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.RouteCode)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(e => e.VehicleMaxWeightCapacity)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.VehicleMaxVolumeCapacity)
            .HasPrecision(18, 2)
            .IsRequired();
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        builder.Property(e => e.Status)
            .HasConversion<int>()
            .IsRequired();

        // CRITICAL: Configure the backing field for ShipmentType so EF Core persists it
        builder.Property("_shipmentType")
            .HasColumnName("ShipmentType")
            .HasConversion<int>()
            .IsRequired();
        
        // Optional destination location for inter-depot shipments
        // NULL = last-mile delivery (door-to-door)
        // NOT NULL = inter-depot shipment to specific location
        builder.Property(e => e.DestinationLocationId)
            .IsRequired(false);
        
        // Configure relationship with Packages
        // NOTE: CurrentShipmentId is the FK property in Package
        builder.HasMany(e => e.Packages)
            .WithOne()
            .HasForeignKey("CurrentShipmentId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

