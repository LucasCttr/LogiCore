using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities.States;
using System;
using LogiCore.Domain.ValueObjects;

namespace LogiCore.Domain.Entities;

public class Package
{
    public Guid Id { get; private set; }
    public string TrackingNumber { get; private set; } = null!;
    public Recipient Recipient { get; private set; } = null!;
    public decimal Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public PackageStatus Status { get; private set; }

    // Link to the Identity user who created the package
    public string? ApplicationUserId { get; private set; }

    // Navigation property (optional)
    public ApplicationUser? ApplicationUser { get; private set; }

    public static Package Create(string trackingNumber, Recipient recipient, decimal weight, string? applicationUserId)
    {
        // Valdiations - Domain Exceptions
        if (weight <= 0) throw new PackageWeightException("Weight must be greater than zero!.");
        if (string.IsNullOrWhiteSpace(trackingNumber)) throw new DomainException("Invalid Tracking Number.");
        if (recipient is null) throw new DomainException("Recipient is required.");

        return new Package
        {
            Id = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            Recipient = recipient,
            Weight = weight,
            CreatedAt = DateTime.UtcNow,
            ApplicationUserId = applicationUserId,
            Status = PackageStatus.Pending
        };
    }

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

    internal void SetStatus(PackageStatus status)
    {
        Status = status;
    }

    private IPackageState GetState()
    {
        return Status switch
        {
            PackageStatus.Pending => new States.PendingState(),
            PackageStatus.InTransit => new States.InTransitState(),
            PackageStatus.Delivered => new States.DeliveredState(),
            PackageStatus.Canceled => new States.CanceledState(),
            _ => throw new DomainException("Unknown package status")
        };
    }

    public void StartTransit()
    {
        GetState().StartTransit(this);
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