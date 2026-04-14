namespace LogiCore.Domain.Entities;

/// <summary>
/// Defines the type of shipment based on its destination:
/// - Transfer: Shipment to another depot (inter-depot)
/// - LastMile: Shipment for final delivery to customer door (last-mile)
/// </summary>
public enum ShipmentType
{
    Transfer = 1,
    LastMile = 2
}
