using System;
using LogiCore.Domain.Entities;

namespace LogiCore.Domain.Common.Events;

public sealed class PackageStatusChangedEvent : IDomainEvent
{
    public Guid PackageId { get; init; }
    public PackageStatus OldStatus { get; init; }
    public PackageStatus NewStatus { get; init; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    
    // Context information
    public string? UserId { get; init; }                // Who triggered the action
    public int? LocationId { get; init; }              // Where the action occurred
    public Guid? ShipmentId { get; init; }             // Associated shipment
    public string? Notes { get; init; }                // Human-readable description
}
