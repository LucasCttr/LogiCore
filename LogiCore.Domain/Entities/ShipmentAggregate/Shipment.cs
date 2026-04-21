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
        Console.WriteLine($"[Domain.Shipment.Create] Called with:");
        Console.WriteLine($"[Domain.Shipment.Create]   routeCode: {routeCode}");
        Console.WriteLine($"[Domain.Shipment.Create]   originLocationId: {originLocationId}");
        Console.WriteLine($"[Domain.Shipment.Create]   destinationLocationId: {destinationLocationId}");
        Console.WriteLine($"[Domain.Shipment.Create]   explicit shipmentType: {shipmentType}");
        
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

        // Determine shipment type: explicit parameter takes precedence
        // If not provided, inferred from locationIds
        var type = shipmentType;
        
        if (!type.HasValue)
        {
            // Infer type based on locations:
            // - Pickup: has OriginLocationId (collecting from depot)
            // - Transfer: has both OriginLocationId and DestinationLocationId (depot-to-depot)
            // - LastMile: has only DestinationLocationId (delivery to customer address)
            if (originLocationId.HasValue && destinationLocationId.HasValue)
            {
                type = ShipmentType.Transfer;
                Console.WriteLine($"[Domain.Shipment.Create] INFERRED: Transfer (both locations)");
            }
            else if (originLocationId.HasValue)
            {
                type = ShipmentType.Pickup;
                Console.WriteLine($"[Domain.Shipment.Create] INFERRED: Pickup (only origin)");
            }
            else if (destinationLocationId.HasValue)
            {
                type = ShipmentType.LastMile;
                Console.WriteLine($"[Domain.Shipment.Create] INFERRED: LastMile (only destination)");
            }
            else
            {
                throw new DomainException("Shipment must have at least an origin or destination location.");
            }
        }
        else
        {
            Console.WriteLine($"[Domain.Shipment.Create] USING EXPLICIT TYPE: {type}");
        }

        Console.WriteLine($"[Domain.Shipment.Create] Final type: {type}");

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
            _shipmentType = type!.Value // At this point, type is guaranteed to have a value
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
        Console.WriteLine($"[Domain.MarkAsDelivered] Called for Shipment {Id}, Type: {Type}, Status: {Status}");
        
        if (Status != ShipmentStatus.Dispatched && Status != ShipmentStatus.Arrived)
            throw new DomainException("Only dispatched or arrived shipments can be marked as delivered.");

        DeliveredAt = DateTime.UtcNow;
        Status = ShipmentStatus.Delivered;
        Console.WriteLine($"[Domain.MarkAsDelivered] Shipment status changed to: {Status}");

        // Synchronize packages based on shipment type
        Console.WriteLine($"[Domain.MarkAsDelivered] Shipment Type check: Type={Type}");
        
        if (Type == ShipmentType.Pickup)
        {
            Console.WriteLine($"[Domain.MarkAsDelivered] Executing Pickup logic: SyncCollectedPackagesToDepot()");
            // For Pickup shipments: move collected packages to AtDepot
            SyncCollectedPackagesToDepot();
        }
        else if (Type == ShipmentType.Transfer)
        {
            Console.WriteLine($"[Domain.MarkAsDelivered] Executing Transfer logic: SyncPackagesToDepot() [InTransit->AtDepot]");
            // For Transfer (depot-to-depot): move InTransit to AtDepot
            SyncPackagesToDepot();
        }
        else if (Type == ShipmentType.LastMile)
        {
            Console.WriteLine($"[Domain.MarkAsDelivered] Executing LastMile logic: SyncPackagesToDelivered() [InTransit->Delivered]");
            // For LastMile: move InTransit to Delivered
            SyncPackagesToDelivered();
        }

        Console.WriteLine($"[Domain.MarkAsDelivered] After sync:");
        foreach (var pkg in _packages)
        {
            Console.WriteLine($"[Domain.MarkAsDelivered]   - Package {pkg.Id}: {pkg.Status}");
        }

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
        
        // Pickup shipments: packages don't transition to InTransit
        // They stay Pending/AtDepot until driver scans them (Collected state)
        if (Type == ShipmentType.Pickup)
        {
            Console.WriteLine($"[Domain.SyncPackagesToInTransit] Pickup shipment - NO state change on START");
            return;
        }

        // Move packages to InTransit when shipment starts (Transfer/LastMile only)
        foreach (var package in _packages)
        {
            Console.WriteLine($"[Domain.SyncPackagesToInTransit] Package {package.Id}: {package.Status}");
            
            // Move AtDepot and Pending packages to InTransit
            if (package.Status == PackageStatus.AtDepot || package.Status == PackageStatus.Pending)
            {
                Console.WriteLine($"[Domain.SyncPackagesToInTransit] Moving {package.Status} -> InTransit");
                package.StartTransit();
            }
        }
    }

    /// <summary>
    /// Synchronizes all packages to AtDepot status when shipment arrives at destination.
    /// Only moves packages that are InTransit. Pending packages remain Pending (not collected).
    /// </summary>
    private void SyncPackagesToDepot()
    {
        Console.WriteLine($"[Domain.SyncPackagesToDepot] Called for Shipment {Id}, Type: {Type}");
        foreach (var package in _packages)
        {
            Console.WriteLine($"[Domain.SyncPackagesToDepot] Package {package.Id}: CurrentStatus = {package.Status}");
            
            // Only move packages that are already in transit. Leave pending packages as-is.
            if (package.Status == PackageStatus.InTransit)
            {
                Console.WriteLine($"[Domain.SyncPackagesToDepot] Moving {package.Id} from InTransit -> AtDepot");
                package.MoveToDepot();
            }
            else
            {
                Console.WriteLine($"[Domain.SyncPackagesToDepot] Package {package.Id} not in InTransit, skipping");
            }
            // Pending packages stay Pending - they weren't collected by the driver
        }
    }

    /// <summary>
    /// Synchronizes all collected packages to AtDepot status when Pickup shipment completes.
    /// This transitions packages from Collected → AtDepot when driver finishes the pickup.
    /// Also ensures AtDepot packages remain AtDepot
    /// </summary>
    private void SyncCollectedPackagesToDepot()
    {
        Console.WriteLine($"[Domain.SyncCollectedPackagesToDepot] Called for Pickup Shipment {Id}");
        foreach (var package in _packages)
        {
            Console.WriteLine($"[Domain.SyncCollectedPackagesToDepot] Package {package.Id}: CurrentStatus = {package.Status}");
            
            // Move Collected packages to AtDepot, and ensure AtDepot packages stay AtDepot
            if (package.Status == PackageStatus.Collected || package.Status == PackageStatus.AtDepot)
            {
                if (package.Status != PackageStatus.AtDepot)
                {
                    Console.WriteLine($"[Domain.SyncCollectedPackagesToDepot] Moving {package.Id} from {package.Status} -> AtDepot");
                    package.MoveToDepot();
                }
                else
                {
                    Console.WriteLine($"[Domain.SyncCollectedPackagesToDepot] Package {package.Id} already AtDepot, keeping it");
                }
                // If already AtDepot, keep it there (no state change needed)
            }
            else
            {
                Console.WriteLine($"[Domain.SyncCollectedPackagesToDepot] Package {package.Id} not Collected or AtDepot, skipping");
            }
        }
    }

    /// <summary>
    /// Synchronizes all in-transit packages to Delivered status when Transfer/LastMile shipment completes.
    /// This transitions packages from InTransit → Delivered when driver finishes delivery.
    /// </summary>
    private void SyncPackagesToDelivered()
    {
        Console.WriteLine($"[Domain.SyncPackagesToDelivered] Called for LastMile Shipment {Id}");
        foreach (var package in _packages)
        {
            Console.WriteLine($"[Domain.SyncPackagesToDelivered] Package {package.Id}: CurrentStatus = {package.Status}");
            
            if (package.Status == PackageStatus.InTransit)
            {
                Console.WriteLine($"[Domain.SyncPackagesToDelivered] Moving {package.Id} from InTransit -> Delivered");
                package.DeliverToCenter();
            }
            else
            {
                Console.WriteLine($"[Domain.SyncPackagesToDelivered] Package {package.Id} not in InTransit, skipping");
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
        Console.WriteLine($"[Domain.FinalizeShipment] Called for Shipment {Id}, Type: {Type}, Status: {Status}");
        
        if (Status != ShipmentStatus.Dispatched && Status != ShipmentStatus.Arrived)
            throw new DomainException("Only dispatched or arrived shipments can be finalized.");

        DeliveredAt = DateTime.UtcNow;
        Status = ShipmentStatus.Delivered;
        Console.WriteLine($"[Domain.FinalizeShipment] Status changed to: {Status}");

        // Type-specific finalization logic
        Console.WriteLine($"[Domain.FinalizeShipment] Processing packages based on Type={Type}");
        
        if (Type == ShipmentType.Pickup)
        {
            Console.WriteLine($"[Domain.FinalizeShipment] Executing Pickup logic");
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
                        Console.WriteLine($"[Domain.FinalizeShipment] Moving {package.Id} to {package.Status} -> AtDepot at location {locationId}");
                        package.MoveToDepotAt(locationId.Value);
                    }
                    else
                    {
                        Console.WriteLine($"[Domain.FinalizeShipment] Moving {package.Id} to {package.Status} -> AtDepot (no location)");
                        package.MoveToDepot();
                    }
                }
            }
        }
        else if (Type == ShipmentType.Transfer && DestinationLocationId.HasValue)
        {
            Console.WriteLine($"[Domain.FinalizeShipment] Executing Transfer logic - moving InTransit packages to AtDepot at {DestinationLocationId}");
            // For depot-to-depot transfers: move all InTransit packages to AtDepot at destination
            foreach (var package in _packages)
            {
                Console.WriteLine($"[Domain.FinalizeShipment] Package {package.Id}: Status = {package.Status}");
                
                if (package.Status == PackageStatus.InTransit)
                {
                    Console.WriteLine($"[Domain.FinalizeShipment] Moving {package.Id} from InTransit -> AtDepot at location {DestinationLocationId}");
                    package.MoveToDepotAt(DestinationLocationId.Value);
                }
                else
                {
                    Console.WriteLine($"[Domain.FinalizeShipment] Package {package.Id} not in InTransit, skipping");
                }
            }
        }
        else if (Type == ShipmentType.LastMile)
        {
            Console.WriteLine($"[Domain.FinalizeShipment] Executing LastMile logic - packages keep their status");
            // LastMile: packages keep their current status (delivery is handled individually)
        }

        Console.WriteLine($"[Domain.FinalizeShipment] After processing:");
        foreach (var pkg in _packages)
        {
            Console.WriteLine($"[Domain.FinalizeShipment]   - Package {pkg.Id}: {pkg.Status}");
        }

        AddDomainEvent(new ShipmentDeliveredEvent
        {
            ShipmentId = this.Id,
            OccurredOn = DateTime.UtcNow
        });
    }
}
