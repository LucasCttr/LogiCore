using System;

namespace LogiCore.Application.DTOs;

public record VehicleDto(Guid Id, string Plate, decimal MaxWeightCapacity, decimal MaxVolumeCapacity, bool IsActive);

public record CreateVehicleDto(string Plate, decimal MaxWeightCapacity, decimal MaxVolumeCapacity);

public record UpdateVehicleDto(string Plate, decimal MaxWeightCapacity, decimal MaxVolumeCapacity, bool IsActive);

public record UpdateVehicleStatusDto(string Status);
