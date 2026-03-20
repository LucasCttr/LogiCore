using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LogiCore.Application.Features.Auth;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public LoginUserCommandHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<UserDto>.Failure("Invalid credentials");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Result<UserDto>.Failure("Invalid credentials");

        var userDto = _mapper.Map<UserDto>(user);
        return Result<UserDto>.Success(userDto);
    }
}
