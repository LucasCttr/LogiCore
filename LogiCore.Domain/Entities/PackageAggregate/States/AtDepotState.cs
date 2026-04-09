namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Entities;

internal class AtDepotState : PackageState
{
    public override PackageStatus Status => PackageStatus.AtDepot;

    public override void StartTransit(Package package)
        => package.SetStatus(PackageStatus.InTransit);

    public override void DeliverToCenter(Package package)
        => package.SetStatus(PackageStatus.DeliveredToCenter);

    public override void Cancel(Package package)
        => package.SetStatus(PackageStatus.Canceled);

    public override void ReturnToOrigin(Package package)
        => package.SetStatus(PackageStatus.Returned);

    public override void EnsureCanUpdateDimensions(Package package)
    {
        // allow editing while at depot
    }

    public override void EnsureCanUpdateWeight(Package package, decimal weight)
    {
        // allow editing while at depot
    }

    public override void EnsureCanUpdateRecipientName(Package package, string recipientName)
    {
        // allow editing while at depot
    }
}
