using System;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "ARS";

    private Money() { }

    public Money(decimal amount, string currency = "ARS")
    {
        if (amount < 0) throw new DomainException("Amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");

        Amount = amount;
        Currency = currency;
    }
}
