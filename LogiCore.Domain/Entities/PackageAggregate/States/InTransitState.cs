namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal class InTransitState : PackageState
{
    public override PackageStatus Status => PackageStatus.InTransit;

    // --- ALLOWED ACTIONS ---

    public override void Deliver(Package package)
    {
        // If the package is in transit, it can be delivered
        package.SetStatus(PackageStatus.Delivered);
    }

    public override void Cancel(Package package)
    {
        // If the package is in transit, it can still be canceled (e.g. if the customer calls to stop delivery)
        package.SetStatus(PackageStatus.Canceled);
    }

    // --- OVERRIDES ---
    // Not overriding the EnsureCanUpdate... methods means that no edits are allowed in this state, and the base implementation will throw exceptions if attempted.
    // This reflects the idea that once a package is in transit, you cannot change its weight, recipient, tracking number, or dimensions until it is delivered or canceled.
}