using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Package.CollectPackage;

/// <summary>
/// Command to collect/pick up a package from seller/origin location.
/// Transitions package from Pending → InTransit (loaded in vehicle).
/// </summary>
public record CollectPackageCommand(Guid PackageId) : IRequest<Result<bool>>;
