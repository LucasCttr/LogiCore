using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.AddPackageToShipment;

public class AddPackageToShipmentCommand : IRequest<Result<ShipmentDto>>
{
    public Guid ShipmentId { get; set; }
    public Guid PackageId { get; set; }
}
