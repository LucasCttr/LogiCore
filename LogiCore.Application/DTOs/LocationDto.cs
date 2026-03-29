using System;

namespace LogiCore.Application.DTOs;

public class LocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string? State { get; set; }
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
