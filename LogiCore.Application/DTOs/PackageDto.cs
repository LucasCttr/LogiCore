using LogiCore.Domain.Entities;

namespace LogiCore.Application.DTOs;

// This DTO is used to transfer package data between the application and the API layer.
public record PackageDto(Guid Id, string TrackingNumber, RecipientDto Recipient, decimal Weight, DateTime CreatedAt, string? ApplicationUserId, PackageStatus Status);
