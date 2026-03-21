using System;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.ValueObjects;

public sealed record Dimensions
{
    public decimal LengthCm { get; }
    public decimal WidthCm { get; }
    public decimal HeightCm { get; }

    // Constructor privado para lógica interna y EF
    private Dimensions(decimal lengthCm, decimal widthCm, decimal heightCm)
    {
        LengthCm = lengthCm;
        WidthCm = widthCm;
        HeightCm = heightCm;
    }

    public static Dimensions Create(decimal length, decimal width, decimal height)
    {
        if (length <= 0 || width <= 0 || height <= 0)
            throw new DomainException("Todas las dimensiones deben ser mayores a cero.");
        
        return new Dimensions(length, width, height);
    }

    public decimal VolumeCm3 => LengthCm * WidthCm * HeightCm;

    public decimal DimensionalWeightKg(decimal divisor = 5000m) =>
        Math.Round(VolumeCm3 / divisor, 2);
}
