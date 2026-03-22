using LogiCore.Domain.ValueObjects;

namespace LogiCore.Application.Common.Interfaces;

public interface ICostCalculator
{
    Money CalculateCost(LogiCore.Domain.Entities.Package package);
}
