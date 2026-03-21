using System;
using LogiCore.Domain.Entities;

namespace LogiCore.Domain.Common.Events;

public sealed class PackageStatusChangedEvent : IDomainEvent
{
    public Guid PackageId { get; init; }
    public PackageStatus OldStatus { get; init; }
    public PackageStatus NewStatus { get; init; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
