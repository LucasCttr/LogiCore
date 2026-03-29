using System;
using System.Collections.Generic;
using System.Linq;
using LogiCore.Domain.Common;
using LogiCore.Domain.Common.Events;
using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

namespace LogiCore.Domain.Entities;

public class Shipment : IHasDomainEvents
{
    public Guid Id { get; private set; }
    public string RouteCode { get; private set; } = null!;
    public Guid VehicleId { get; private set; }
    public Vehicle? Vehicle { get; private set; }
    public Guid? DriverId { get; private set; }
    public Driver? Driver { get; private set; }
    public decimal VehicleMaxWeightCapacity { get; private set; }
    public decimal VehicleMaxVolumeCapacity { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime EstimatedDelivery { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? ArrivedAt { get; private set; }
    private readonly List<Package> _packages = new();
    public IReadOnlyCollection<Package> Packages => _packages.AsReadOnly();
    public ShipmentStatus Status { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    protected Shipment() { }

    public static Shipment Create(string routeCode, Guid vehicleId, decimal vehicleMaxWeightCapacity, decimal vehicleMaxVolumeCapacity, DateTime estimatedDelivery)
    {
        if (string.IsNullOrWhiteSpace(routeCode))
            throw new DomainException("Route code is required.");
        if (vehicleId == Guid.Empty)
            throw new DomainException("VehicleId is required for a shipment.");

        if (vehicleMaxWeightCapacity <= 0)
            throw new DomainException("Vehicle max weight capacity must be greater than zero.");

        if (vehicleMaxVolumeCapacity <= 0)
            throw new DomainException("Vehicle max volume capacity must be greater than zero.");

        if (estimatedDelivery < DateTime.UtcNow)
            throw new DomainException("Estimated delivery date cannot be in the past.");

        return new Shipment
        {
            Id = Guid.NewGuid(),
            RouteCode = routeCode.Trim(),
            VehicleId = vehicleId,
            VehicleMaxWeightCapacity = vehicleMaxWeightCapacity,
            VehicleMaxVolumeCapacity = vehicleMaxVolumeCapacity,
            Status = ShipmentStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            EstimatedDelivery = estimatedDelivery
        };
    }

    public void AddPackage(Package package)
    {
        if (package is null) throw new DomainException("Package cannot be null.");

        if (package.Status != PackageStatus.Pending)
            throw new DomainException("Only packages with Pending status can be added to a shipment.");

        var weightCapacity = Vehicle?.MaxWeightCapacity ?? VehicleMaxWeightCapacity;
        if (GetCurrentWeight() + package.Weight > weightCapacity)
            throw new DomainException("The vehicle would exceed its weight capacity (kg).");
            
        // Volume check
        var volumeCapacity = Vehicle?.MaxVolumeCapacity ?? VehicleMaxVolumeCapacity;
        var currentVolume = _packages.Sum(p => p.Dimensions?.VolumeCm3 ?? 0);
        var packageVolume = package.Dimensions?.VolumeCm3 ?? 0m;

        if (currentVolume + packageVolume > volumeCapacity)
            throw new DomainException("The vehicle would exceed its volume capacity (cm3).");

        _packages.Add(package);
    }

    public decimal GetCurrentWeight()
    {
        return _packages.Sum(p => p.Weight);
    }

    public void DispatchShipment()
    {
        if (!_packages.Any())
            throw new DomainException("Cannot dispatch an empty shipment.");

        if (Status != ShipmentStatus.Draft)
            throw new DomainException($"Cannot dispatch a shipment in {Status} status.");

        Status = ShipmentStatus.Dispatched;
        if (ShippedAt == null) ShippedAt = DateTime.UtcNow;

        // Start transit for all packages in the shipment
        foreach (var package in _packages)
        {
            package.StartTransit();
        }

        // Emits a domain event for the shipment dispatch
        AddDomainEvent(new ShipmentDispatchedEvent
        {
            ShipmentId = this.Id,
            RouteCode = this.RouteCode,
            OccurredOn = DateTime.UtcNow
        });
    }

    // Backwards-compatible alias
    public void Dispatch() => DispatchShipment();

    public void AssignDriver(Guid driverId)
    {
        if (driverId == Guid.Empty)
            throw new DomainException("DriverId is required.");

        DriverId = driverId;
    }

    public void MarkAsDelivered()
    {
        if (Status != ShipmentStatus.Dispatched && Status != ShipmentStatus.Arrived)
            throw new DomainException("Only dispatched or arrived shipments can be marked as delivered.");

        DeliveredAt = DateTime.UtcNow;
        Status = ShipmentStatus.Delivered;

        AddDomainEvent(new ShipmentDeliveredEvent
        {
            ShipmentId = this.Id,
            OccurredOn = DateTime.UtcNow
        });
    }

    public void MarkAsArrived()
    {
        if (Status != ShipmentStatus.Dispatched)
            throw new DomainException("Only dispatched shipments can be marked as arrived.");

        ArrivedAt = DateTime.UtcNow;
        Status = ShipmentStatus.Arrived;

        AddDomainEvent(new ShipmentArrivedEvent
        {
            ShipmentId = this.Id,
            OccurredOn = DateTime.UtcNow
        });
    }

    public void Cancel()
    {
        if (Status == ShipmentStatus.Dispatched || Status == ShipmentStatus.Arrived || Status == ShipmentStatus.Delivered)
            throw new DomainException("Cannot cancel a shipment that is already dispatched or completed.");

        Status = ShipmentStatus.Canceled;
        AddDomainEvent(new ShipmentCanceledEvent
        {
            ShipmentId = this.Id,
            OccurredOn = DateTime.UtcNow
        });
    }
}
