using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.CreateShipment;

public class CreateShipmentCommand : IRequest<Result<ShipmentDto>>
{
    public required Guid DriverId { get; init; }
    public required Guid VehicleId { get; init; }
    public required List<Guid> PackageIds { get; init; }
    public required DateTime EstimatedDelivery { get; init; }
}
