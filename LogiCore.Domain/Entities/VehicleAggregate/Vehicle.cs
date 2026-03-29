using System;
using System.Collections.Generic;
using LogiCore.Domain.Common;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.Entities;

public enum VehicleStatus
{
    Active = 0,
    InRepair = 1,
    OutOfService = 2
}

public class Vehicle : IHasDomainEvents
{
    public Guid Id { get; private set; }
    public string Plate { get; private set; } = null!;
    public decimal MaxWeightCapacity { get; private set; }
    public decimal MaxVolumeCapacity { get; private set; }
    public bool IsActive { get; private set; }
    public VehicleStatus Status { get; private set; }
    public List<Shipment> Shipments { get; private set; } = new();

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    protected Vehicle() { }

    public static Vehicle Create(string plate, decimal maxWeight, decimal maxVolume)
    {
        if (string.IsNullOrWhiteSpace(plate))
            throw new DomainException("The plate number is required.");

        if (maxWeight <= 0)
            throw new DomainException("The weight capacity must be greater than zero.");

        if (maxVolume <= 0)
            throw new DomainException("The volume capacity must be greater than zero.");

        return new Vehicle
        {
            Id = Guid.NewGuid(),
            Plate = plate.ToUpper().Trim(),
            MaxWeightCapacity = maxWeight,
            MaxVolumeCapacity = maxVolume,
            IsActive = true,
            Status = VehicleStatus.Active
        };
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        if (active)
            Status = VehicleStatus.Active;
        else if (Status == VehicleStatus.Active)
            Status = VehicleStatus.OutOfService;
    }

    public void SetStatus(VehicleStatus status)
    {
        Status = status;
        IsActive = status == VehicleStatus.Active;
    }
}
