namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Entities;

internal interface IPackageState
{
    PackageStatus Status { get; }
    void StartTransit(Package package);
    void MoveToDepot(Package package);
    void Deliver(Package package);
    void DeliverToCenter(Package package);
    void Cancel(Package package);
    void EnsureCanUpdateWeight(Package package, decimal weight);
    void EnsureCanUpdateTrackingNumber(Package package, string trackingNumber);
    void EnsureCanUpdateRecipientName(Package package, string recipientName);
    void EnsureCanUpdateDimensions(Package package);
}
