using System;

namespace LogiCore.Domain.Entities;

public class PackageStatusHistory
{
    public Guid Id { get; set; }
    public Guid PackageId { get; set; }
    public PackageStatus FromStatus { get; set; }
    public PackageStatus ToStatus { get; set; }
    public DateTime OccurredAt { get; set; }
    
    // User who triggered the action (FK to AspNetUsers.Id)
    public string? UserId { get; set; }
    
    // Context: where and what
    public int? LocationId { get; set; }  // Current location depot (nullable)
    public Guid? ShipmentId { get; set; } // Associated shipment (nullable)
    
    // Readable description of what happened
    public string? Notes { get; set; }
    
    // Legacy compatibility
    [Obsolete("Use UserId instead")]
    public string? EmployeeId { get; set; }
    public string? InternalNotes { get; set; }
}
