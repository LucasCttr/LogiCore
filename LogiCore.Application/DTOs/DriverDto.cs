using System;

namespace LogiCore.Application.DTOs;

public class DriverDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string LicenseNumber { get; set; } = null!;
    public bool IsActive { get; set; }
}
