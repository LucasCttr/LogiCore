using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LogiCore.Application.Features.Auth;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;
    private readonly LogiCore.Application.Common.Interfaces.Security.IJwtProvider _jwtProvider;
    private readonly LogiCore.Application.Common.Interfaces.Security.IRefreshTokenService _refreshTokenService;
    private readonly Microsoft.Extensions.Logging.ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(UserManager<ApplicationUser> userManager, IMapper mapper, LogiCore.Application.Common.Interfaces.Security.IJwtProvider jwtProvider, LogiCore.Application.Common.Interfaces.Security.IRefreshTokenService refreshTokenService, Microsoft.Extensions.Logging.ILogger<LoginUserCommandHandler> logger)
    {
        _userManager = userManager;
        _mapper = mapper;
        _jwtProvider = jwtProvider;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for Email='{Email}'", request?.Email ?? "(null)");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponseDto>.Failure("Invalid credentials");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Result<AuthResponseDto>.Failure("Invalid credentials");

        // Include user roles in token so role-based authorization works
        var roles = await _userManager.GetRolesAsync(user);
        var additionalClaims = roles.Select(r => new KeyValuePair<string, string>(ClaimTypes.Role, r));

        var tokenString = _jwtProvider.CreateToken(user.Id, user.Email ?? string.Empty, additionalClaims);

        // create a refresh token persisted in DB
        var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);

        var userDto = _mapper.Map<UserDto>(user);
        var authResponse = new AuthResponseDto(tokenString, userDto, refreshToken);
        return Result<AuthResponseDto>.Success(authResponse);
    }
}
