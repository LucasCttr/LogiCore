using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.StartShipment;

/// <summary>
/// Marks a shipment as active (Dispatched)
/// Only Draft shipments can be started
/// Driver can only have one active shipment at a time
/// Scanner flow removed - shipment starts immediately without scanning
/// </summary>
public class StartShipmentCommand : IRequest<Result<bool>>
{
    public Guid ShipmentId { get; set; }
    public List<string>? ScannedPackageIds { get; set; } // Optional - for backwards compatibility
}
