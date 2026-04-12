using System;
using System.Collections.Generic;
using System.Linq;
using LogiCore.Domain.Common;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.Entities;

public class Driver : IHasDomainEvents
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string LicenseNumber { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public string ApplicationUserId { get; private set; } = null!; 

    public ApplicationUser? ApplicationUser { get; private set; }
    private readonly List<Shipment> _shipments = new();
    public IReadOnlyCollection<Shipment> Shipments => _shipments.AsReadOnly();

    // Domain Events
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Constructor for EF Core
    protected Driver() { }

    // Factory Method: The only entry point for creating a Driver
    public static Driver Create(string name, string licenseNumber, string applicationUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Driver name is required.");
        
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new DomainException("License number is required.");
        
        if (string.IsNullOrWhiteSpace(applicationUserId))
            throw new DomainException("ApplicationUserId is required for a Driver.");

        return new Driver
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            LicenseNumber = licenseNumber.Trim(),
            ApplicationUserId = applicationUserId,
            IsActive = true
        };
    }

    // MMethods for behavior (Domain Logic)
    public void SetActive(bool active) => IsActive = active;

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Driver name is required.");
        Name = name.Trim();
    }

    public void SetLicenseNumber(string licenseNumber)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new DomainException("License number is required.");
        LicenseNumber = licenseNumber.Trim();
    }

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}