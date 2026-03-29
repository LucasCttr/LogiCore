using System;
using LogiCore.Domain.Common;

namespace LogiCore.Domain.Common.Events;

public record ShipmentArrivedEvent : IDomainEvent
{
    public Guid ShipmentId { get; init; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
