namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal class CanceledState : IPackageState
{
    public PackageStatus Status => PackageStatus.Canceled;

    public void StartTransit(Package package)
    {
        throw new DomainException("Cannot start transit for a canceled package.");
    }

    public void Deliver(Package package)
    {
        throw new DomainException("Cannot deliver a canceled package.");
    }

    public void Cancel(Package package)
    {
        throw new DomainException("Package is already canceled.");
    }

    public void EnsureCanUpdateWeight(Package package, decimal weight)
    {
        throw new DomainException("Cannot modify the weight of a canceled package.");
    }

    public void EnsureCanUpdateTrackingNumber(Package package, string trackingNumber)
    {
        throw new DomainException("Cannot update tracking number for a canceled package.");
    }

    public void EnsureCanUpdateRecipientName(Package package, string recipientName)
    {
        throw new DomainException("Cannot update recipient name for a canceled package.");
    }
}
