using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Shipment;

public record BulkAssignToShipmentCommand(Guid ShipmentId, IEnumerable<Guid> PackageIds) : IRequest<Result<bool>>;
