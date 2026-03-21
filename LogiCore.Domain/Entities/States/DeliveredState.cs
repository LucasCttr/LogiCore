namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal class DeliveredState : IPackageState
{
    public PackageStatus Status => PackageStatus.Delivered;

    public void StartTransit(Package package)
    {
        throw new DomainException("Cannot transition a delivered package back to transit.");
    }

    public void Deliver(Package package)
    {
        throw new DomainException("Package is already delivered.");
    }

    public void Cancel(Package package)
    {
        throw new DomainException("Cannot cancel a delivered package.");
    }

    public void EnsureCanUpdateWeight(Package package, decimal weight)
    {
        throw new DomainException("Cannot modify the weight of a delivered package.");
    }

    public void EnsureCanUpdateTrackingNumber(Package package, string trackingNumber)
    {
        throw new DomainException("Cannot update tracking number for a delivered package.");
    }

    public void EnsureCanUpdateRecipientName(Package package, string recipientName)
    {
        throw new DomainException("Cannot update recipient name for a delivered package.");
    }
}
