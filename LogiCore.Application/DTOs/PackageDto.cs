using LogiCore.Domain.Entities;

namespace LogiCore.Application.DTOs;

// This DTO is used to transfer package data between the application and the API layer.
public record DimensionsDto(decimal LengthCm, decimal WidthCm, decimal HeightCm, decimal VolumeCm3);

public record PackageDto
{
	public Guid Id { get; init; }
	public string TrackingNumber { get; init; } = null!;
	public string? Description { get; init; }
	public string? InternalCode { get; init; }
	public RecipientDto Recipient { get; init; } = null!;
	public decimal Weight { get; init; }
	public DateTime CreatedAt { get; init; }
	public string? ApplicationUserId { get; init; }
	public PackageStatus Status { get; init; }
	public DimensionsDto? Dimensions { get; init; }
	public string? OriginAddress { get; init; }
	public string? DestinationAddress { get; init; }
}
