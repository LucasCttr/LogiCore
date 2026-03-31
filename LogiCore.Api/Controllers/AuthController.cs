using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Features.Auth;
using LogiCore.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using LogiCore.Domain.Entities;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly LogiCore.Application.Common.Interfaces.Security.IRefreshTokenService _refreshTokenService;
    private readonly LogiCore.Application.Common.Interfaces.Security.IJwtProvider _jwtProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(IMediator mediator, IMapper mapper, LogiCore.Application.Common.Interfaces.Security.IRefreshTokenService refreshTokenService, LogiCore.Application.Common.Interfaces.Security.IJwtProvider jwtProvider, UserManager<ApplicationUser> userManager)
    {
        _mediator = mediator;
        _mapper = mapper;
        _refreshTokenService = refreshTokenService;
        _jwtProvider = jwtProvider;
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Result<UserDto>>> Register([FromBody] RegisterUserCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<AuthResponseDto>>> Login([FromBody] LoginUserCommand request)
    {
        var result = await _mediator.Send(request);
        // If login succeeded and a refresh token was returned in the DTO, set it as an HttpOnly cookie for silent refresh
        if (result.IsSuccess && result.Value?.RefreshToken is not null)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // set to true in production (https)
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(30)
            };
            Response.Cookies.Append("refreshToken", result.Value.RefreshToken, cookieOptions);
        }
        return result;
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<Result<AuthResponseDto>>> Refresh([FromBody] RefreshRequest? body)
    {
        // Check cookie first, then body
        var refreshToken = Request.Cookies["refreshToken"] ?? body?.RefreshToken;
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result<AuthResponseDto>.Failure("Missing refresh token");

        var user = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);
        if (user == null)
            return Result<AuthResponseDto>.Failure("Invalid refresh token");

        // include roles in token
        var roles = await _userManager.GetRolesAsync(user);
        var additionalClaims = roles.Select(r => new KeyValuePair<string, string>(ClaimTypes.Role, r));
        var newAccessToken = _jwtProvider.CreateToken(user.Id, user.Email ?? string.Empty, additionalClaims);

        // rotate refresh token: revoke old + create new
        await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken);
        var newRefresh = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);

        var userDto = _mapper.Map<UserDto>(user);
        var authResponse = new AuthResponseDto(newAccessToken, userDto, newRefresh);

        // set new cookie
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(30)
        };
        Response.Cookies.Append("refreshToken", newRefresh, cookieOptions);

        return Result<AuthResponseDto>.Success(authResponse);
    }

    public record RefreshRequest(string? RefreshToken);
}
