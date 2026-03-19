using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Packages;
public class CreatePackageCommand : IRequest<Result<PackageDto>>
{
    public required string TrackingNumber { get; init; }
    public required string RecipientName { get; init; }
    public decimal Weight { get; init; }
}