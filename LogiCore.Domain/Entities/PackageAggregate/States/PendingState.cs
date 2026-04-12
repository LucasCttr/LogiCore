namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal class PendingState : PackageState
{
    public override PackageStatus Status => PackageStatus.Pending;

    // --- ALLOWED ACTIONS ---

    public override void StartTransit(Package package)
    {
        throw new DomainException("Package must be in AtDepot status to start transit. Move package to depot first.");
    }

    public override void MoveToDepot(Package package)
        => package.SetStatus(PackageStatus.AtDepot);

    public override void Cancel(Package package) 
        => package.SetStatus(PackageStatus.Canceled);

    public override void ReturnToOrigin(Package package)
        => package.SetStatus(PackageStatus.Returned);


    public override void EnsureCanUpdateDimensions(Package package) 
    { 
        // OK: Allows setting dimensions when creating the package
    }

    public override void EnsureCanUpdateWeight(Package package, decimal weight)
    {
        // OK:  Allows setting weight when creating the package
    }

    public override void EnsureCanUpdateRecipientName(Package package, string name)
    {
        // OK:  Allows setting recipient name when creating the package
    }
}