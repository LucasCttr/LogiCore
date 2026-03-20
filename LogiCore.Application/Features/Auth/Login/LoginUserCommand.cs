using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Auth;

public class LoginUserCommand : IRequest<Result<UserDto>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
