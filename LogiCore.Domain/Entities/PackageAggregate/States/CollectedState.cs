namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Entities;

internal class CollectedState : PackageState
{
    public override PackageStatus Status => PackageStatus.Collected;

    public override void MoveToDepot(Package package)
        => package.SetStatus(PackageStatus.AtDepot);

    public override void Cancel(Package package)
        => package.SetStatus(PackageStatus.Canceled);

    public override void ReturnToOrigin(Package package)
        => package.SetStatus(PackageStatus.Returned);

    public override void EnsureCanUpdateDimensions(Package package)
    {
        // Allow editing while collected (package is still in vehicle, not delivered yet)
    }

    public override void EnsureCanUpdateWeight(Package package, decimal weight)
    {
        // Allow editing while collected
    }

    public override void EnsureCanUpdateRecipientName(Package package, string recipientName)
    {
        // Allow editing while collected
    }
}
