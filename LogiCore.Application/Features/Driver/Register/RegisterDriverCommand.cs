using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Driver.Register;

public class RegisterDriverCommand : IRequest<Result<DriverDto>>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;

    // Driver-specific
    public string LicenseNumber { get; set; } = null!;
    public string Phone { get; set; } = null!;
}
