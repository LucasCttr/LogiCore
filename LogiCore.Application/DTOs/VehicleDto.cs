using System;

namespace LogiCore.Application.DTOs;

public class VehicleDto
{
    public Guid Id { get; set; }
    public string? LicensePlate { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public decimal MaxWeightCapacity { get; set; }
    public decimal MaxVolumeCapacity { get; set; }
    public bool IsActive { get; set; }
}

public record CreateVehicleDto(string Plate, string? Make, string? Model, decimal MaxWeightCapacity, decimal MaxVolumeCapacity);

public record UpdateVehicleDto(string Plate, string? Make, string? Model, decimal MaxWeightCapacity, decimal MaxVolumeCapacity, bool IsActive);

public record UpdateVehicleStatusDto(string Status);
