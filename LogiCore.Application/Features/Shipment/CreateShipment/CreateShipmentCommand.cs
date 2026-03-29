using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.CreateShipment;

public class CreateShipmentCommand : IRequest<Result<ShipmentDto>>
{
    public string RouteCode { get; set; } = null!;
    public Guid VehicleId { get; set; }
    public DateTime EstimatedDelivery { get; set; }
}
