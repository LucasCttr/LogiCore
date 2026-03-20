using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LogiCore.Application.Features.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _modelMapper;

    public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager, IMapper modelMapper)
    {
        _userManager = userManager;
        _modelMapper = modelMapper;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Mapping command to entity
        var user = _modelMapper.Map<ApplicationUser>(request);

        // 2. Create user with password
        var result = await _userManager.CreateAsync(user, request.Password);

        // 3. Check if creation was successful
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "Error en el registro";
            return Result<UserDto>.Failure(error);
        }

        // 4. Mapping created entity to output DTO
        var userDto = _modelMapper.Map<UserDto>(user);

        return Result<UserDto>.Success(userDto);
    }
}