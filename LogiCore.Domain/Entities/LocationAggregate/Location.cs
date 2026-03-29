using System;
using LogiCore.Domain.Common;
using LogiCore.Domain.Common.Exceptions;

namespace LogiCore.Domain.Entities;

public class Location : IHasDomainEvents
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string AddressLine1 { get; private set; } = null!;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = null!;
    public string? State { get; private set; }
    public string PostalCode { get; private set; } = null!;
    public string Country { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    protected Location() { }

    public static Location Create(string name, string addressLine1, string? addressLine2, string city, string? state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");
        if (string.IsNullOrWhiteSpace(addressLine1)) throw new DomainException("AddressLine1 is required.");
        if (string.IsNullOrWhiteSpace(city)) throw new DomainException("City is required.");
        if (string.IsNullOrWhiteSpace(postalCode)) throw new DomainException("PostalCode is required.");
        if (string.IsNullOrWhiteSpace(country)) throw new DomainException("Country is required.");

        return new Location
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            AddressLine1 = addressLine1.Trim(),
            AddressLine2 = string.IsNullOrWhiteSpace(addressLine2) ? null : addressLine2.Trim(),
            City = city.Trim(),
            State = string.IsNullOrWhiteSpace(state) ? null : state.Trim(),
            PostalCode = postalCode.Trim(),
            Country = country.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}
