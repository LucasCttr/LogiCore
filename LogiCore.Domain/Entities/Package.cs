using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.Entities;

public class Package
{
    public Guid Id { get; private set; }
    public string TrackingNumber { get; private set; } = null!;
    public string RecipientName { get; private set; } = null!;
    public decimal Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Constructor para inicializar el objeto
    public Package(string trackingNumber, string recipientName, decimal weight)
    {
        if (weight <= 0)
        throw new PackageWeightException("Weight must be greater than zero.");
        
        Id = Guid.NewGuid();
        TrackingNumber = trackingNumber;
        RecipientName = recipientName;
        Weight = weight;
        CreatedAt = DateTime.UtcNow;
    }

    // .NET necesita un constructor vacío para Entity Framework (EF)
    protected Package() { } 
}