using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Packages;

[System.Obsolete("ShipPackageCommand is deprecated. Use Shipment-based flow: add package to shipment and dispatch the shipment.")]
public record ShipPackageCommand(Guid PackageId) : IRequest<Result<PackageDto>>;