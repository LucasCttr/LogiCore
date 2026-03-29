using LogiCore.Domain.Entities;
using System.Collections.Generic;

namespace LogiCore.Application.DTOs;

public class ShipmentDto
{
	public Guid Id { get; set; }
	public string RouteCode { get; set; } = null!;
	public Guid VehicleId { get; set; }
	public Guid? DriverId { get; set; }
	public decimal VehicleMaxWeightCapacity { get; set; }
	public decimal VehicleMaxVolumeCapacity { get; set; }
	public IEnumerable<Guid> PackageIds { get; set; } = Enumerable.Empty<Guid>();
	public ShipmentStatus Status { get; set; }
}
