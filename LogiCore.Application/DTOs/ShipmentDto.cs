using LogiCore.Domain.Entities;
using System.Collections.Generic;

namespace LogiCore.Application.DTOs;

public class ShipmentDto
{
	public Guid Id { get; set; }
	public string RouteCode { get; set; } = null!;
	public Guid VehicleId { get; set; }
	public Guid? DriverId { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime EstimatedDelivery { get; set; }
	public DateTime? ShippedAt { get; set; }
	public DateTime? DeliveredAt { get; set; }
	public DateTime? ArrivedAt { get; set; }
	public decimal VehicleMaxWeightCapacity { get; set; }
	public decimal VehicleMaxVolumeCapacity { get; set; }
	public int? OriginLocationId { get; set; }
	public string? OriginLocationName { get; set; }
	public int? DestinationLocationId { get; set; }
	public string? DestinationLocationName { get; set; }
	public ShipmentType? Type { get; set; }
	public IEnumerable<Guid> PackageIds { get; set; } = Enumerable.Empty<Guid>();
	public ShipmentStatus Status { get; set; }
}
