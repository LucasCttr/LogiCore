using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Packages;
public class CreatePackageCommand : IRequest<Result<PackageDto>>
{
    // Basic fields used by the simplified frontend
    public string TrackingNumber { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? Weight { get; init; }

    // Free-text origin/destination
    public string? Origin { get; init; }
    public string? Destination { get; init; }

    // Legacy recipient fields (optional) - kept for compatibility with other clients
    public string? RecipientName { get; init; }
    public string? RecipientAddress { get; init; }
    public string? RecipientPhone { get; init; }
    public string? RecipientFloorApartment { get; init; }
    public string? RecipientCity { get; init; }
    public string? RecipientProvince { get; init; }
    public string? RecipientPostalCode { get; init; }
    public string? RecipientDni { get; init; }

    // Optional dimensions
    public decimal? LengthCm { get; init; }
    public decimal? WidthCm { get; init; }
    public decimal? HeightCm { get; init; }
}