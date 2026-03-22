using LogiCore.Domain.ValueObjects;

namespace LogiCore.Domain.Common.Interfaces;

public interface ICostCalculator
{
    Money CalculateCost(LogiCore.Domain.Entities.Package package);
}
