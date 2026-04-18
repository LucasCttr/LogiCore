namespace LogiCore.Domain.Entities;

/// <summary>
/// Defines the type of shipment:
/// - Pickup: Collection of packages from customers/locations
/// - Transfer: Shipment to another depot (inter-depot)
/// - LastMile: Shipment for final delivery to customer door (last-mile)
/// </summary>
public enum ShipmentType
{
    Pickup = 0,
    Transfer = 1,
    LastMile = 2
}
