using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public record BulkCancelPackagesCommand(IEnumerable<Guid> PackageIds, bool ReturnToOrigin = false) : IRequest<Result<bool>>;
