namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal abstract class PackageState : IPackageState
{
    public abstract PackageStatus Status { get; }

    public virtual void StartTransit(Package package)
        => throw new DomainException($"Cannot start transit from state {Status}.");

    public virtual void Collect(Package package)
        => throw new DomainException($"Cannot collect package from state {Status}.");

    public virtual void MoveToDepot(Package package)
        => throw new DomainException($"Cannot move package to depot from state {Status}.");

    public virtual void Deliver(Package package)
        => throw new DomainException($"Cannot deliver package from state {Status}.");

    public virtual void DeliverToCenter(Package package)
        => throw new DomainException($"Cannot deliver package to center from state {Status}.");

    public virtual void Cancel(Package package)
        => throw new DomainException($"Cannot cancel package from state {Status}.");

    public virtual void ReturnToOrigin(Package package)
        => throw new DomainException($"Cannot return package to origin from state {Status}.");

    public virtual void EnsureCanUpdateWeight(Package package, decimal weight)
        => throw new DomainException($"Cannot update weight in state {Status}.");

    public virtual void EnsureCanUpdateTrackingNumber(Package package, string trackingNumber)
        => throw new DomainException($"Cannot update tracking number in state {Status}.");

    public virtual void EnsureCanUpdateRecipientName(Package package, string recipientName)
        => throw new DomainException($"Cannot update recipient name in state {Status}.");

    public virtual void EnsureCanUpdateDimensions(Package package)
        => throw new DomainException($"Cannot update dimensions in state {Status}.");
}
