using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities.States;
using System;
using LogiCore.Domain.ValueObjects;
using System.Collections.Generic;
using LogiCore.Domain.Common;
using LogiCore.Domain.Common.Events;

namespace LogiCore.Domain.Entities;

public class Package : IHasDomainEvents
{
    public Guid Id { get; private set; }
    public string TrackingNumber { get; private set; } = null!;
    public Recipient Recipient { get; private set; } = null!;
    public decimal Weight { get; private set; }
    public string Description { get; private set; }
    public string InternalCode { get; private set; }
    public string OriginAddress { get; private set; }
    public string DestinationAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public PackagePriority Priority { get; private set; } = PackagePriority.Standard;
    // Representa dónde está físicamente (Depósito A, Sucursal B, etc.)
    public int? CurrentLocationId { get; private set; }

    // Representa en qué viaje está (si Status es InTransit)
    public Guid? CurrentShipmentId { get; private set; }
    public PackageStatus Status { get; private set; }

    // Link to the Identity user who created the package
    public string ApplicationUserId { get; private set; }

    public static Package Create(string trackingNumber, Recipient recipient, decimal weight, string applicationUserId, LogiCore.Domain.ValueObjects.Dimensions dimensions, string description, string internalCode, string originAddress, string destinationAddress, PackagePriority priority = PackagePriority.Standard)
    {
        // Valdiations - Domain Exceptions
        if (weight <= 0) throw new PackageWeightException("Weight must be greater than zero!.");
        if (string.IsNullOrWhiteSpace(trackingNumber)) throw new DomainException("Invalid Tracking Number.");
        if (recipient is null) throw new DomainException("Recipient is required.");

        var now = DateTime.UtcNow;
        var package = new Package
        {
            Id = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            Recipient = recipient,
            Weight = weight,
            CreatedAt = now,
            LastUpdatedAt = now,
            Priority = priority,
            ApplicationUserId = applicationUserId,
            Status = PackageStatus.Pending,
            Description = description,
            InternalCode = internalCode,
            OriginAddress = originAddress,
            DestinationAddress = destinationAddress
        };

        package._dimensions = dimensions;
        package.SyncState(package.Status);

        // Generate initial status change event for audit trail
        package.AddDomainEvent(new PackageStatusChangedEvent
        {
            PackageId = package.Id,
            OldStatus = (PackageStatus)(-1), // -1 to indicate "created" (no previous status)
            NewStatus = PackageStatus.Pending,
            OccurredOn = now,
            UserId = applicationUserId,
            Notes = "Package registered in system"
        });

        return package;
    }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    private IPackageState _state;
    private Dimensions _dimensions;
    private Money _estimatedCost;

    public Dimensions Dimensions => _dimensions;

    public void UpdateWeight(decimal weight)
    {
        GetState().EnsureCanUpdateWeight(this, weight);

        if (weight <= 0) throw new PackageWeightException("Weight must be greater than zero.");
        Weight = weight;
    }

    public void UpdateTrackingNumber(string trackingNumber)
    {
        GetState().EnsureCanUpdateTrackingNumber(this, trackingNumber);

        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new DomainException("Invalid Tracking Number.");

        TrackingNumber = trackingNumber;
    }

    public void UpdateRecipient(Recipient recipient)
    {
        GetState().EnsureCanUpdateRecipientName(this, recipient?.Name ?? string.Empty);

        if (recipient is null) throw new DomainException("Recipient is required.");

        Recipient = recipient;
    }

    public void UpdateDimensions(Dimensions dimensions)
    {
        GetState().EnsureCanUpdateDimensions(this);
        if (dimensions is null) throw new DomainException("Dimensions are required.");
        _dimensions = dimensions;
    }

    public Money EstimatedCost => _estimatedCost;

    public void ApplyShippingCost(LogiCore.Domain.Common.Interfaces.ICostCalculator calculator)
    {
        if (calculator is null) throw new DomainException("Cost calculator is required.");

        // Ensure package is in a state that allows recalculation (reuse existing state checks)
        GetState().EnsureCanUpdateWeight(this, this.Weight);

        var cost = calculator.CalculateCost(this);
        _estimatedCost = new LogiCore.Domain.ValueObjects.Money(cost.Amount, cost.Currency);
    }

    internal void SetStatus(PackageStatus status)
    {
        if (Status == status) return;

        var old = Status;
        Status = status;
        LastUpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PackageStatusChangedEvent
        {
            PackageId = this.Id,
            OldStatus = old,
            NewStatus = status,
            OccurredOn = DateTime.UtcNow
        });

        SyncState(status);
    }

    private IPackageState GetState()
    {
        if (_state is null)
        {
            _state = Status switch
            {
                PackageStatus.Pending => new States.PendingState(),
                PackageStatus.InTransit => new States.InTransitState(),
                PackageStatus.Delivered => new States.DeliveredState(),
                PackageStatus.Canceled => new States.CanceledState(),
                PackageStatus.AtDepot => new States.AtDepotState(),
                PackageStatus.DeliveredToCenter => new States.DeliveredToCenterState(),
                PackageStatus.Returned => new States.ReturnedState(),
                PackageStatus.Collected => new States.CollectedState(),
                _ => throw new DomainException("Unknown package status")
            };
        }

        return _state;
    }

    private void SyncState(PackageStatus newStatus)
    {
        _state = newStatus switch
        {
            PackageStatus.Pending => new PendingState(),
            PackageStatus.InTransit => new InTransitState(),
            PackageStatus.Delivered => new DeliveredState(),
            PackageStatus.Canceled => new CanceledState(),
            PackageStatus.AtDepot => new States.AtDepotState(),
            PackageStatus.DeliveredToCenter => new States.DeliveredToCenterState(),
            PackageStatus.Returned => new States.ReturnedState(),
            PackageStatus.Collected => new States.CollectedState(),
            _ => throw new DomainException("Unknown package status")
        };
    }


    /// <summary>
    /// Moves the package to a depot location.
    /// Delegates state validation to the current state via the State Pattern.
    /// </summary>
    public void MoveToDepot()
    {
        GetState().MoveToDepot(this);
        CurrentShipmentId = null; // Al entrar al depósito, baja del camión
    }

    /// <summary>
    /// Moves the package to a specific depot location with tracking.
    /// Delegates state validation to the current state via the State Pattern.
    /// </summary>
    public void MoveToDepotAt(int locationId)
    {
        if (locationId <= 0)
            throw new DomainException("Invalid location ID.");

        MoveToDepot();
        CurrentLocationId = locationId;
    }

    /// <summary>
    /// Delivers the package to a distribution center.
    /// Delegates state validation to the current state via the State Pattern.
    /// </summary>
    public void DeliverToCenter()
    {
        GetState().DeliverToCenter(this);
    }

    /// <summary>
    /// Delivers the package to a specific distribution center with tracking.
    /// Delegates state validation to the current state via the State Pattern.
    /// </summary>
    public void DeliverToCenterAt(int locationId)
    {
        if (locationId <= 0)
            throw new DomainException("Invalid location ID.");

        DeliverToCenter();
        CurrentLocationId = locationId;
    }

    /// <summary>
    /// Assigns the package to a shipment, moving it to InTransit status.
    /// Delegates state validation to the current state via the State Pattern.
    /// </summary>
    public void AssignToShipment(Guid shipmentId)
    {
        if (shipmentId == Guid.Empty)
            throw new DomainException("Invalid shipment ID.");

        // Only assign the shipment ID, don't change package status yet.
        // Packages will transition to InTransit when the driver starts the shipment.
        CurrentShipmentId = shipmentId;
    }

    /// <summary>
    /// Marks the package as Collected.
    /// Called when driver physically picks up the package during a route.
    /// Note: CurrentLocationId is NOT set when collected - it's set only when arriving at AtDepot.
    /// </summary>
    public void CollectAt(int locationId)
    {
        // For now, just mark as collected. Location only assigned at AtDepot.
        this.Collect();
    }

    /// <summary>
    /// Returns the package to its origin.
    /// Delegates state validation to the current state via the State Pattern.
    /// </summary>
    public void ReturnToOrigin()
    {
        if (Status == PackageStatus.Returned) return; // Idempotent
        GetState().ReturnToOrigin(this);
    }

    public void StartTransit()
    {
        GetState().StartTransit(this);
    }

    /// <summary>
    /// Marks the package as collected (for Pickup shipment types).
    /// Delegates state validation to the current state via the State Pattern.
    /// </summary>
    public void Collect()
    {
        GetState().Collect(this);
    }

    public void Deliver()
    {
        GetState().Deliver(this);
    }

    public void Cancel()
    {
        GetState().Cancel(this);
    }

    protected Package() { }
}