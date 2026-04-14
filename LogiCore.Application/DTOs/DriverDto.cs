using System;

namespace LogiCore.Application.DTOs;

public class DriverDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string LicenseNumber { get; set; } = null!;
    public bool IsActive { get; set; }
    public string? ApplicationUserId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid? AssignedVehicleId { get; set; }
    public AssignedVehicleInfoDto? AssignedVehicle { get; set; }
}

public class AssignedVehicleInfoDto
{
    public Guid Id { get; set; }
    public string? LicensePlate { get; set; }
    public string? Model { get; set; }
    public string? Make { get; set; }
}
