namespace LogiCore.Application.DTOs;

/// <summary>
/// DTO with minimal package information for scanner mode validation during depot ingress.
/// </summary>
public class PackageForScannerDto
{
    public Guid Id { get; set; }
    public string TrackingNumber { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusLabel { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string? OriginAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public string? RecipientName { get; set; }
}
