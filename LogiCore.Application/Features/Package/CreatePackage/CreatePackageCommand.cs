using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Packages;
public class CreatePackageCommand : IRequest<Result<PackageDto>>
{
    public required string TrackingNumber { get; init; }
    public required string RecipientName { get; init; }
    public required string RecipientAddress { get; init; }
    public required string RecipientPhone { get; init; }
    public string? RecipientFloorApartment { get; init; }
    public required string RecipientCity { get; init; }
    public required string RecipientProvince { get; init; }
    public required string RecipientPostalCode { get; init; }
    public required string RecipientDni { get; init; }
    public required decimal Weight { get; init; }
    public required decimal LengthCm { get; init; }
    public required decimal WidthCm { get; init; }
    public required decimal HeightCm { get; init; }
}