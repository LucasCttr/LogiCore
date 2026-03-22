namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal class CanceledState : PackageState
{
    public override PackageStatus Status => PackageStatus.Canceled;

}
