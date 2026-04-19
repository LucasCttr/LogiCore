using LogiCore.Domain.Entities;

namespace LogiCore.Application.DTOs;

/// <summary>
/// DTO for current shipment info in package context
/// </summary>
public record CurrentShipmentDto
{
    public Guid Id { get; init; }
    public ShipmentType Type { get; init; }
    public string? DestinationName { get; init; } // Location name or null for last-mile
    public int? DestinationLocationId { get; init; }
}

/// <summary>
/// Extended package DTO with current shipment context
/// Used for GetPackage endpoint to inform the frontend about the delivery type
/// </summary>
public record PackageDetailDto
{
    public Guid Id { get; init; }
    public string TrackingNumber { get; init; } = null!;
    public string? Description { get; init; }
    public string? InternalCode { get; init; }
    public RecipientDto Recipient { get; init; } = null!;
    public decimal Weight { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastUpdatedAt { get; init; }
    public PackagePriority Priority { get; init; }
    public string? ApplicationUserId { get; init; }
    public PackageStatus Status { get; init; }
    public DimensionsDto? Dimensions { get; init; }
    public string? OriginAddress { get; init; }
    public string? DestinationAddress { get; init; }
    public CurrentShipmentDto? CurrentShipment { get; init; }
    public int? CurrentLocationId { get; init; }
}
