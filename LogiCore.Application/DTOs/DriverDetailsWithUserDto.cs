namespace LogiCore.Application.DTOs;

public record DriverDetailsWithUserDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public Guid? DriverId { get; init; }
    
    // User info
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsUserActive { get; init; }
    
    // Driver details
    public string LicenseNumber { get; init; } = string.Empty;
    public string LicenseType { get; init; } = string.Empty;
    public DateTime LicenseExpiry { get; init; }
    public DateTime InsuranceExpiry { get; init; }
    public Guid? AssignedVehicleId { get; init; }
    public string? AssignedVehiclePlate { get; init; }
    public string? AssignedVehicleMake { get; init; }
    public string? AssignedVehicleModel { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
