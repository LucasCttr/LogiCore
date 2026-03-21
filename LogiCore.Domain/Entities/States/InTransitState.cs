namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal class InTransitState : IPackageState
{
    public PackageStatus Status => PackageStatus.InTransit;

    public void StartTransit(Package package)
    {
        throw new DomainException("Package is already in transit.");
    }

    public void Deliver(Package package)
    {
        package.SetStatus(PackageStatus.Delivered);
    }

    public void Cancel(Package package)
    {
        package.SetStatus(PackageStatus.Canceled);
    }

    public void EnsureCanUpdateWeight(Package package, decimal weight)
    {
        throw new DomainException("Cannot update weight for a package in transit.");
    }

    public void EnsureCanUpdateTrackingNumber(Package package, string trackingNumber)
    {
        throw new DomainException("Cannot update tracking number for a package in transit.");
    }

    public void EnsureCanUpdateRecipientName(Package package, string recipientName)
    {
        throw new DomainException("Cannot update recipient name for a package in transit.");
    }
}
