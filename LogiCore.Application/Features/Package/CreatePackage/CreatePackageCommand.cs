using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Packages;
public class CreatePackageCommand : IRequest<Result<PackageDto>>
{
    // Basic fields used by the simplified frontend
    public string? TrackingNumber { get; init; } // Auto-generated if null
    public required string Description { get; init; }
    public required string InternalCode { get; init; }
    public required decimal Weight { get; init; }

    // Free-text origin/destination
    public required string Origin { get; init; }
    public required string Destination { get; init; }

    // Legacy recipient fields (now required)
    public required string RecipientName { get; init; }
    public required string RecipientAddress { get; init; }
    public required string RecipientPhone { get; init; }
    public required string RecipientFloorApartment { get; init; }
    public required string RecipientCity { get; init; }
    public required string RecipientProvince { get; init; }
    public required string RecipientPostalCode { get; init; }
    public required string RecipientDni { get; init; }

    // Dimensions now required
    public required decimal LengthCm { get; init; }
    public required decimal WidthCm { get; init; }
    public required decimal HeightCm { get; init; }
}