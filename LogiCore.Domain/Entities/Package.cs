namespace LogiCore.Domain.Entities;

public class Package
{
    public Guid Id { get; private set; }
    public string TrackingNumber { get; private set; }
    public string RecipientName { get; private set; }
    public double Weight { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Constructor para inicializar el objeto
    public Package(string trackingNumber, string recipientName, double weight)
    {
        Id = Guid.NewGuid();
        TrackingNumber = trackingNumber;
        RecipientName = recipientName;
        Weight = weight;
        CreatedAt = DateTime.UtcNow;
    }

    // .NET necesita un constructor vacío para Entity Framework (EF)
    protected Package() { } 
}