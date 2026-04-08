using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Shipment;

public record BulkDeliverToCenterCommand(IEnumerable<Guid> PackageIds) : IRequest<Result<bool>>;
