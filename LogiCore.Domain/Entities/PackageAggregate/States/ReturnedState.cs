namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal class ReturnedState : PackageState
{
    public override PackageStatus Status => PackageStatus.Returned;

    public override void EnsureCanUpdateDimensions(Package package)
        => throw new DomainException($"Cannot update dimensions in state {Status}.");

    public override void EnsureCanUpdateWeight(Package package, decimal weight)
        => throw new DomainException($"Cannot update weight in state {Status}.");

    public override void EnsureCanUpdateRecipientName(Package package, string recipientName)
        => throw new DomainException($"Cannot update recipient name in state {Status}.");
}
