using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public record BulkMoveToDepotCommand(IEnumerable<Guid> PackageIds) : IRequest<Result<bool>>;
