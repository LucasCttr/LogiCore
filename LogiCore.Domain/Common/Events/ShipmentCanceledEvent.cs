using System;
using LogiCore.Domain.Common;

namespace LogiCore.Domain.Common.Events;

public record ShipmentCanceledEvent : IDomainEvent
{
    public Guid ShipmentId { get; init; }
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
