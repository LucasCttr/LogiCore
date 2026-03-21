using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public record ShipPackageCommand(Guid PackageId) : IRequest<Result<PackageDto>>;