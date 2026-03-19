namespace LogiCore.Api.Models.DTOs;

public record UpdatePackageRequest(
    string TrackingNumber,
    string RecipientName,
    decimal Weight
);