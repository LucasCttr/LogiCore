using System;

namespace LogiCore.Application.DTOs;

public record PackageInternalHistoryDto(
    string FromStatus, 
    string ToStatus, 
    DateTime OccurredAt, 
    string? UserId,
    string? UserName,        // Username (email)
    string? FirstName,       // Employee first name
    string? LastName,        // Employee last name
    string? UserRoles,       // Comma-separated roles: "Admin", "Driver", etc.
    int? LocationId,
    Guid? ShipmentId,
    string? Notes,
    string? EmployeeId,      // Legacy
    string? InternalNotes    // Legacy
)
{
    public Guid Id { get; init; }
    public Guid PackageId { get; init; }
}
