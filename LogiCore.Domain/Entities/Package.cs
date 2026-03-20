using LogiCore.Domain.Common.Exceptions;
using System;

namespace LogiCore.Domain.Entities;

public class Package
{
    public Guid Id { get; private set; }
    public string TrackingNumber { get; private set; } = null!;
    public string RecipientName { get; private set; } = null!;
    public decimal Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Package Create(string trackingNumber, string recipientName, decimal weight)
    {
        // Valdiations - Domain Exceptions
        if (weight <= 0) throw new PackageWeightException("Weight must be greater than zero!.");
        if (string.IsNullOrWhiteSpace(trackingNumber)) throw new DomainException("Invalid Tracking Number.");
        if (string.IsNullOrWhiteSpace(recipientName)) throw new DomainException("Recipient Name is required.");

        return new Package
        {
            Id = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            RecipientName = recipientName,
            Weight = weight,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateWeight(decimal weight)
    {
        if (weight <= 0) throw new PackageWeightException("Weight must be greater than zero.");
        Weight = weight;
    }

    public void UpdateTrackingNumber(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new DomainException("Invalid Tracking Number.");

        TrackingNumber = trackingNumber;
    }

    public void UpdateRecipientName(string recipientName)
    {
        if (string.IsNullOrWhiteSpace(recipientName))
            throw new DomainException("Recipient Name is required.");

        RecipientName = recipientName;
    }

    protected Package() { } 
}