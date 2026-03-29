using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.CompleteShipment;

public class CompleteShipmentCommand : IRequest<Result<bool>>
{
    public Guid ShipmentId { get; set; }
}
