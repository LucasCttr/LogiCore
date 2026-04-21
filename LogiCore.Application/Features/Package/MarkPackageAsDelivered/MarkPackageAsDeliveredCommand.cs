using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Package.MarkPackageAsDelivered;

/// <summary>
/// Command to mark a single package as delivered by the driver.
/// This is invoked when the driver delivers a package to the customer's address.
/// </summary>
public record MarkPackageAsDeliveredCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// The ID of the package to mark as delivered.
    /// Assigned from route parameter in controller.
    /// </summary>
    public Guid PackageId { get; init; }

    /// <summary>
    /// Optional delivery notes or comments from the driver.
    /// </summary>
    public string? DeliveryNotes { get; init; }

    /// <summary>
    /// Geolocation latitude of the delivery (for tracking).
    /// </summary>
    public decimal? Latitude { get; init; }

    /// <summary>
    /// Geolocation longitude of the delivery (for tracking).
    /// </summary>
    public decimal? Longitude { get; init; }
}
