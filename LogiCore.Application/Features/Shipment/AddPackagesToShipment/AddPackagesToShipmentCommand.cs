using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.AddPackagesToShipment;

public class AddPackagesToShipmentCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// The ID of the shipment to add packages to.
    /// </summary>
    public required Guid ShipmentId { get; init; }

    /// <summary>
    /// List of package IDs to add to the shipment.
    /// </summary>
    public required List<Guid> PackageIds { get; init; }
}
