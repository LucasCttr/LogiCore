using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Driver.Update;

public class UpdateDriverCommand : IRequest<Result<DriverDto>>
{
    public Guid DriverId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Phone { get; set; }
}
