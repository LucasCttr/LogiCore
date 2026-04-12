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
}
