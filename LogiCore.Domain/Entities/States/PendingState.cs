namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal class PendingState : IPackageState
{
    public PackageStatus Status => PackageStatus.Pending;

    public void StartTransit(Package package)
    {
        package.SetStatus(PackageStatus.InTransit);
    }

    public void Deliver(Package package)
    {
        throw new DomainException("Cannot deliver a package that is pending.");
    }

    public void Cancel(Package package)
    {
        package.SetStatus(PackageStatus.Canceled);
    }

    public void EnsureCanUpdateWeight(Package package, decimal weight)
    {
        // Pending packages can be updated
    }

    public void EnsureCanUpdateTrackingNumber(Package package, string trackingNumber)
    {
        // Pending packages can be updated
    }

    public void EnsureCanUpdateRecipientName(Package package, string recipientName)
    {
        // Pending packages can be updated
    }
}
