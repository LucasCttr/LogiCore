namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal class PendingState : PackageState
{
    public override PackageStatus Status => PackageStatus.Pending;

}
