namespace LogiCore.Api.Models.DTOs;

public record CreatePackageRequest(
    string TrackingNumber,
    string RecipientName,
    decimal Weight
);
