using LogiCore.Domain.Entities;
using System.Collections.Generic;

namespace LogiCore.Application.DTOs;

public record ShipmentDto(Guid Id, string RouteCode, Guid VehicleId, decimal VehicleMaxWeightCapacity, decimal VehicleMaxVolumeCapacity, IEnumerable<Guid> PackageIds, ShipmentStatus Status);
