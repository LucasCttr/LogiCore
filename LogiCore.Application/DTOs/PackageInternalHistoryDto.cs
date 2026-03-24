using System;

namespace LogiCore.Application.DTOs;

public record PackageInternalHistoryDto(string FromStatus, string ToStatus, DateTime OccurredAt, string? EmployeeId, string? InternalNotes)
{
    public Guid Id { get; init; }
    public Guid PackageId { get; init; }
}
