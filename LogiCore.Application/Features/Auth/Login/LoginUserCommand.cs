using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Auth;

public class LoginUserCommand : IRequest<Result<AuthResponseDto>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
