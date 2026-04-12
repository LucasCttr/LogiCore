using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Package.MoveToDepot;

public record MovePackageToDepotCommand(Guid PackageId) : IRequest<Result<bool>>;
