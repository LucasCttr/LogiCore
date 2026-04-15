using System;
using LogiCore.Domain.Common;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.Entities;

/// <summary>
/// Represents detailed information for a user with the Driver role.
/// This is a 1:1 relationship with ApplicationUser.
/// </summary>
public class DriverDetails : IHasDomainEvents
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = null!;
    public string LicenseNumber { get; private set; } = null!;
    public string LicenseType { get; private set; } = null!; // A, B, C, etc.
    public DateTime LicenseExpiry { get; private set; }
    public DateTime InsuranceExpiry { get; private set; }
    public Guid? AssignedVehicleId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // Navigation
    public ApplicationUser? User { get; private set; }
    public Vehicle? AssignedVehicle { get; private set; }

    // Domain Events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Constructor for EF Core
    protected DriverDetails() { }

    /// <summary>
    /// Factory method to create DriverDetails for a new driver user.
    /// </summary>
    public static DriverDetails Create(
        string userId,
        string licenseNumber,
        string licenseType,
        DateTime licenseExpiry,
        DateTime insuranceExpiry)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new DomainException("UserId is required.");
        
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new DomainException("License number is required.");
        
        if (string.IsNullOrWhiteSpace(licenseType))
            throw new DomainException("License type is required.");
        
        if (licenseExpiry < DateTime.UtcNow)
            throw new DomainException("License expiry date must be in the future.");
        
        if (insuranceExpiry < DateTime.UtcNow)
            throw new DomainException("Insurance expiry date must be in the future.");

        return new DriverDetails
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            LicenseNumber = licenseNumber.Trim(),
            LicenseType = licenseType.Trim(),
            LicenseExpiry = licenseExpiry,
            InsuranceExpiry = insuranceExpiry,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Domain Methods
    public void UpdateLicenseInfo(string licenseNumber, string licenseType, DateTime licenseExpiry)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new DomainException("License number is required.");
        
        if (licenseExpiry < DateTime.UtcNow)
            throw new DomainException("License expiry date must be in the future.");

        LicenseNumber = licenseNumber.Trim();
        LicenseType = licenseType.Trim();
        LicenseExpiry = licenseExpiry;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateInsuranceExpiry(DateTime insuranceExpiry)
    {
        if (insuranceExpiry < DateTime.UtcNow)
            throw new DomainException("Insurance expiry date must be in the future.");

        InsuranceExpiry = insuranceExpiry;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignVehicle(Guid? vehicleId)
    {
        AssignedVehicleId = vehicleId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
