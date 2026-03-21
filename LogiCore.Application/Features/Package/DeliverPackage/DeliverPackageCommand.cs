using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public record DeliverPackageCommand(Guid Id) : IRequest<Result<Unit>>;