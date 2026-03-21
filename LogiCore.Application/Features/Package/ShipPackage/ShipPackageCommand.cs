using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public record ShipPackageCommand(Guid Id) : IRequest<Result<Unit>>;