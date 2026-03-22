using System;
using LogiCore.Domain.Common.Interfaces;
using LogiCore.Domain.ValueObjects;

namespace LogiCore.Infrastructure.Services;

public class StandardCostCalculator : ICostCalculator
{
    private const decimal PricePerKg = 150.0m; // ARS per kg example
    private const decimal VolumetricFactor = 5000.0m;

    public Money CalculateCost(LogiCore.Domain.Entities.Package package)
    {
        if (package is null) throw new ArgumentNullException(nameof(package));

        var dims = package.Dimensions;
        decimal volumetricWeight = 0m;

        if (dims != null)
        {
            volumetricWeight = (dims.LengthCm * dims.WidthCm * dims.HeightCm) / VolumetricFactor;
        }

        var chargingWeight = Math.Max(package.Weight, volumetricWeight);
        var amount = Math.Round(chargingWeight * PricePerKg, 2);

        return new Money(amount, "ARS");
    }
}
