using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.AssignDriver;

public class AssignDriverToShipmentCommand : IRequest<Result<bool>>
{
    public Guid ShipmentId { get; set; }
    public Guid DriverId { get; set; }
}
