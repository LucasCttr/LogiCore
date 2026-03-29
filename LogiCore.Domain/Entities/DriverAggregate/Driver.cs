using System;
using System.Collections.Generic;
using LogiCore.Domain.Common;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.Entities;

public class Driver : IHasDomainEvents
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string LicenseNumber { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public List<Shipment> Shipments { get; private set; } = new();

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    protected Driver() { }

    public static Driver Create(string name, string licenseNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Driver name is required.");
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new DomainException("License number is required.");

        return new Driver
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            LicenseNumber = licenseNumber.Trim(),
            IsActive = true
        };
    }

    public void SetActive(bool active) => IsActive = active;
}
