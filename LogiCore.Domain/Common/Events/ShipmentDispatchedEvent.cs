using System;
using LogiCore.Domain.Common;

namespace LogiCore.Domain.Common.Events;

public record ShipmentDispatchedEvent : IDomainEvent
{
    public Guid ShipmentId { get; init; }
    public string RouteCode { get; init; } = null!;
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
