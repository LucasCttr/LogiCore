using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public record CancelPackageCommand(Guid PackageId) : IRequest<Result<PackageDto>>;