namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal abstract class PackageState : IPackageState
{
    public abstract PackageStatus Status { get; }

    public virtual void StartTransit(Package package)
        => throw new DomainException($"Cannot start transit from state {Status}.");

    public virtual void Deliver(Package package)
        => throw new DomainException($"Cannot deliver package from state {Status}.");

    public virtual void Cancel(Package package)
        => throw new DomainException($"Cannot cancel package from state {Status}.");

    public virtual void EnsureCanUpdateWeight(Package package, decimal weight)
        => throw new DomainException($"Cannot update weight in state {Status}.");

    public virtual void EnsureCanUpdateTrackingNumber(Package package, string trackingNumber)
        => throw new DomainException($"Cannot update tracking number in state {Status}.");

    public virtual void EnsureCanUpdateRecipientName(Package package, string recipientName)
        => throw new DomainException($"Cannot update recipient name in state {Status}.");
}
