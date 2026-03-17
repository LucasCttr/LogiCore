using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.Entities;

public class Package
{
    public Guid Id { get; private set; }
    public string TrackingNumber { get; private set; } = null!;
    public string RecipientName { get; private set; } = null!;
    public decimal Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Factory estática para creación controlada y validación del dominio
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

    protected Package() { }
}