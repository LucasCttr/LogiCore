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
    // Optional origin location: For Pickup shipments, the depot where packages are collected
    public int? OriginLocationId { get; private set; }
    // Optional destination location: NULL = last-mile delivery (door-to-door), NOT NULL = inter-depot shipment
    public int? DestinationLocationId { get; private set; }
    private ShipmentType _shipmentType;
    private readonly List<Package> _packages = new();
    public IReadOnlyCollection<Package> Packages => _packages.AsReadOnly();
    public ShipmentStatus Status { get; private set; }

    /// <summary>
    /// Gets the shipment type explicitly set during creation
    /// </summary>
    public ShipmentType Type => _shipmentType;

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    protected Shipment() { }

    public static Shipment Create(string routeCode, Guid vehicleId, decimal vehicleMaxWeightCapacity, decimal vehicleMaxVolumeCapacity, DateTime estimatedDelivery, int? originLocationId = null, int? destinationLocationId = null, ShipmentType? shipmentType = null)
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

        // Determine shipment type: explicit parameter or inferred from locationIds
        var type = shipmentType ?? (originLocationId.HasValue ? ShipmentType.Pickup : destinationLocationId.HasValue ? ShipmentType.Transfer : ShipmentType.LastMile);

        return new Shipment
        {
            Id = Guid.NewGuid(),
            RouteCode = routeCode.Trim(),
            VehicleId = vehicleId,
            VehicleMaxWeightCapacity = vehicleMaxWeightCapacity,
            VehicleMaxVolumeCapacity = vehicleMaxVolumeCapacity,
            Status = ShipmentStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            EstimatedDelivery = estimatedDelivery,
            OriginLocationId = originLocationId,
            DestinationLocationId = destinationLocationId,
            _shipmentType = type
        };
    }

    /// <summary>
    /// Adds a single package to the shipment with validation.
    /// </summary>
    public void AddPackage(Package package)
    {
        if (package is null) throw new DomainException("Package cannot be null.");

        if (package.Status != PackageStatus.AtDepot)
            throw new DomainException("Only packages with AtDepot status can be assigned to a shipment.");

        ValidateCapacity(package);
        package.AssignToShipment(this.Id);
        _packages.Add(package);
    }

    /// <summary>
    /// Adds multiple packages to the shipment in bulk.
    /// All packages must be valid before any are added (atomic operation).
    /// </summary>
    public void AddPackages(IEnumerable<Package> packages)
    {
        if (packages is null) throw new DomainException("Packages collection cannot be null.");

        var packagesList = packages.ToList();
        if (!packagesList.Any()) throw new DomainException("At least one package is required.");

        // Validate all packages before adding any (fail-fast)
        foreach (var package in packagesList)
        {
            if (package is null) throw new DomainException("Package cannot be null.");
            
            if (package.Status != PackageStatus.AtDepot && package.Status != PackageStatus.Pending)
                throw new DomainException($"Package {package.TrackingNumber} has invalid status. Only Pending or AtDepot packages can be assigned to a shipment.");
        }

        // Validate cumulative capacity
        var totalWeight = GetCurrentWeight() + packagesList.Sum(p => p.Weight);
        var weightCapacity = Vehicle?.MaxWeightCapacity ?? VehicleMaxWeightCapacity;
        if (totalWeight > weightCapacity)
            throw new DomainException($"Adding these packages would exceed weight capacity. Required: {totalWeight}kg, Capacity: {weightCapacity}kg");

        var totalVolume = _packages.Sum(p => p.Dimensions?.VolumeCm3 ?? 0) + 
                         packagesList.Sum(p => p.Dimensions?.VolumeCm3 ?? 0m);
        var volumeCapacity = Vehicle?.MaxVolumeCapacity ?? VehicleMaxVolumeCapacity;
        if (totalVolume > volumeCapacity)
            throw new DomainException($"Adding these packages would exceed volume capacity. Required: {totalVolume}cm3, Capacity: {volumeCapacity}cm3");

        // All validations passed, add all packages
        foreach (var package in packagesList)
        {
            package.AssignToShipment(this.Id);
            _packages.Add(package);
        }
    }

    /// <summary>
    /// Validates if a package can be added based on vehicle capacity.
    /// </summary>
    private void ValidateCapacity(Package package)
    {
        var weightCapacity = Vehicle?.MaxWeightCapacity ?? VehicleMaxWeightCapacity;
        if (GetCurrentWeight() + package.Weight > weightCapacity)
            throw new DomainException("The vehicle would exceed its weight capacity (kg).");

        var volumeCapacity = Vehicle?.MaxVolumeCapacity ?? VehicleMaxVolumeCapacity;
        var currentVolume = _packages.Sum(p => p.Dimensions?.VolumeCm3 ?? 0);
        var packageVolume = package.Dimensions?.VolumeCm3 ?? 0m;

        if (currentVolume + packageVolume > volumeCapacity)
            throw new DomainException("The vehicle would exceed its volume capacity (cm3).");
    }

    public decimal GetCurrentWeight()
    {
        return _packages.Sum(p => p.Weight);
    }

    /// <summary>
    /// Dispatches the shipment and transitions all packages to InTransit status.
    /// </summary>
    public void DispatchShipment()
    {
        Console.WriteLine($"[Domain.DispatchShipment] Called for shipment {Id}");
        Console.WriteLine($"[Domain.DispatchShipment] Shipment Type: {Type}");
        Console.WriteLine($"[Domain.DispatchShipment] Packages count: {_packages.Count}");
        
        if (!_packages.Any())
            throw new DomainException("Cannot dispatch an empty shipment.");

        if (Status != ShipmentStatus.Draft)
            throw new DomainException($"Cannot dispatch a shipment in {Status} status.");

        Status = ShipmentStatus.Dispatched;
        if (ShippedAt == null) ShippedAt = DateTime.UtcNow;

        Console.WriteLine($"[Domain.DispatchShipment] Calling SyncPackagesToInTransit");
        
        // Synchronize all packages to InTransit status
        SyncPackagesToInTransit();

        Console.WriteLine($"[Domain.DispatchShipment] After sync:");
        foreach (var pkg in _packages)
        {
            Console.WriteLine($"[Domain.DispatchShipment]   - Package {pkg.Id}: {pkg.Status}");
        }

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

    /// <summary>
    /// Marks the shipment as arrived at final destination.
    /// Synchronizes packages to AtDepot status based on shipment type:
    /// - Pickup: Collected packages → AtDepot
    /// - Other types: InTransit packages → AtDepot
    /// </summary>
    public void MarkAsArrived()
    {
        if (Status != ShipmentStatus.Dispatched)
            throw new DomainException("Only dispatched shipments can be marked as arrived.");

        ArrivedAt = DateTime.UtcNow;
        Status = ShipmentStatus.Arrived;

        // Synchronize packages based on shipment type
        if (Type == ShipmentType.Pickup)
        {
            SyncCollectedPackagesToDepot();
        }
        else
        {
            SyncPackagesToDepot();
        }

        AddDomainEvent(new ShipmentArrivedEvent
        {
            ShipmentId = this.Id,
            OccurredOn = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Cancels the shipment and all its packages.
    /// </summary>
    public void Cancel()
    {
        if (Status == ShipmentStatus.Dispatched || Status == ShipmentStatus.Arrived || Status == ShipmentStatus.Delivered)
            throw new DomainException("Cannot cancel a shipment that is already dispatched or completed.");

        Status = ShipmentStatus.Canceled;

        // Synchronize all packages to Canceled status
        foreach (var package in _packages)
        {
            package.Cancel();
        }

        AddDomainEvent(new ShipmentCanceledEvent
        {
            ShipmentId = this.Id,
            OccurredOn = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Synchronizes all packages to InTransit status when shipment is dispatched.
    /// </summary>
    /// <summary>
    /// Synchronizes all packages to InTransit status when dispatching non-Pickup shipments.
    /// For Pickup shipments, packages remain their current state (Pending/Collected).
    /// </summary>
    private void SyncPackagesToInTransit()
    {
        Console.WriteLine($"[Domain.SyncPackagesToInTransit] Called. Type: {Type}");
        
        // For non-Pickup shipments, move all packages to InTransit
        if (Type != ShipmentType.Pickup)
        {
            Console.WriteLine($"[Domain.SyncPackagesToInTransit] Non-Pickup shipment, updating packages");
            foreach (var package in _packages)
            {
                Console.WriteLine($"[Domain.SyncPackagesToInTransit] Before: Package {package.Id} Status = {package.Status}");
                package.StartTransit();
                Console.WriteLine($"[Domain.SyncPackagesToInTransit] After: Package {package.Id} Status = {package.Status}");
            }
        }
        else
        {
            Console.WriteLine($"[Domain.SyncPackagesToInTransit] Pickup shipment, packages staying in current state");
        }
        // For Pickup shipments, packages stay in their current state (Pending/Collected)
    }

    /// <summary>
    /// Synchronizes all packages to AtDepot status when shipment arrives at destination.
    /// Only moves packages that are InTransit. Pending packages remain Pending (not collected).
    /// </summary>
    private void SyncPackagesToDepot()
    {
        foreach (var package in _packages)
        {
            // Only move packages that are already in transit. Leave pending packages as-is.
            if (package.Status == PackageStatus.InTransit)
            {
                package.MoveToDepot();
            }
            // Pending packages stay Pending - they weren't collected by the driver
        }
    }

    /// <summary>
    /// Synchronizes all collected packages to AtDepot status when Pickup shipment completes.
    /// This transitions packages from Collected → AtDepot when driver finishes the pickup.
    /// </summary>
    private void SyncCollectedPackagesToDepot()
    {
        foreach (var package in _packages)
        {
            if (package.Status == PackageStatus.Collected)
            {
                package.MoveToDepot();
            }
        }
    }

    /// <summary>
    /// Finalizes the shipment based on its type:
    /// - Pickup: Marks as Delivered and moves Collected/Pending packages to AtDepot at their current location
    /// - Transfer: Marks as Delivered and updates all InTransit packages to AtDepot at destination location
    /// - LastMile: Marks as Delivered without changing package statuses
    /// </summary>
    public void FinalizeShipment()
    {
        if (Status != ShipmentStatus.Dispatched && Status != ShipmentStatus.Arrived)
            throw new DomainException("Only dispatched or arrived shipments can be finalized.");

        DeliveredAt = DateTime.UtcNow;
        Status = ShipmentStatus.Delivered;

        // Type-specific finalization logic
        if (Type == ShipmentType.Pickup)
        {
            // For pickup: move collected/pending packages to AtDepot
            // Use shipment's destination location as fallback (where the pickup delivery was sent)
            foreach (var package in _packages)
            {
                if (package.Status == PackageStatus.Collected || package.Status == PackageStatus.Pending)
                {
                    // Prefer package's current location, fallback to shipment's destination
                    int? locationId = package.CurrentLocationId ?? DestinationLocationId;
                    if (locationId.HasValue && locationId > 0)
                    {
                        package.MoveToDepotAt(locationId.Value);
                    }
                    else
                    {
                        // If no location available, just change status without location
                        package.MoveToDepot();
                    }
                }
            }
        }
        else if (Type == ShipmentType.Transfer && DestinationLocationId.HasValue)
        {
            // For depot-to-depot transfers: move all InTransit packages to AtDepot at destination
            foreach (var package in _packages)
            {
                if (package.Status == PackageStatus.InTransit)
                {
                    package.MoveToDepotAt(DestinationLocationId.Value);
                }
            }
        }
        // For LastMile: packages keep their current status
        // (Delivery statuses are determined by individual package operations)

        AddDomainEvent(new ShipmentDeliveredEvent
        {
            ShipmentId = this.Id,
            OccurredOn = DateTime.UtcNow
        });
    }
}
