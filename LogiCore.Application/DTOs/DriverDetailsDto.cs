namespace LogiCore.Application.DTOs;

public record DriverDetailsDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string LicenseNumber { get; init; } = string.Empty;
    public string LicenseType { get; init; } = string.Empty;
    public DateTime LicenseExpiry { get; init; }
    public DateTime InsuranceExpiry { get; init; }
    public Guid? AssignedVehicleId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
