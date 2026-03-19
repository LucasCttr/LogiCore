using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.Entities;

public class Package
{
    public Guid Id { get; private set; }
    public string TrackingNumber { get; private set; } = null!;
    public string RecipientName { get; private set; } = null!;
    public decimal Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Factory method to create a new package with validation
    public static Package Create(string trackingNumber, string recipientName, decimal weight)
    {
        if (weight <= 0) throw new PackageWeightException("Weight must be greater than zero.");
        if (string.IsNullOrWhiteSpace(trackingNumber)) throw new ArgumentException("Tracking inválido.", nameof(trackingNumber));

        return new Package
        {
            Id = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            RecipientName = recipientName,
            Weight = weight,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Methods to update fields with encapsulated validation
    public void UpdateTrackingNumber(string trackingNumber)
    {
        if (string.IsNullOrWhiteSpace(trackingNumber)) throw new ArgumentException("Tracking inválido.", nameof(trackingNumber));
        TrackingNumber = trackingNumber;
    }

    public void UpdateRecipientName(string recipientName)
    {
        if (string.IsNullOrWhiteSpace(recipientName)) throw new ArgumentException("Recipient name inválido.", nameof(recipientName));
        RecipientName = recipientName;
    }

    public void UpdateWeight(decimal weight)
    {
        if (weight <= 0) throw new PackageWeightException("Weight must be greater than zero.");
        Weight = weight;
    }

    protected Package() { }
}