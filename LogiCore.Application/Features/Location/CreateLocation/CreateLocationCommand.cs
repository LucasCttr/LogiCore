using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using System;

namespace LogiCore.Application.Features.Location.CreateLocation;

public class CreateLocationCommand : IRequest<Result<LocationDto>>
{
    public string Name { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = null!;
    public string? State { get; set; }
    public string PostalCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}
