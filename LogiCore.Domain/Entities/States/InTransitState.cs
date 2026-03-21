namespace LogiCore.Domain.Entities.States;

using LogiCore.Domain.Common.Exceptions;
using LogiCore.Domain.Entities;

internal class InTransitState : PackageState
{
    public override PackageStatus Status => PackageStatus.InTransit;

}
