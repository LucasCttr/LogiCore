using System;

namespace LogiCore.Domain.Entities;

public class PackageStatusHistory
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public PackageStatus FromStatus { get; set; }
    public PackageStatus ToStatus { get; set; }
    public DateTime OccurredAt { get; set; }
    // Optional auditing information
    public string? EmployeeId { get; set; }
    public string? InternalNotes { get; set; }
}
