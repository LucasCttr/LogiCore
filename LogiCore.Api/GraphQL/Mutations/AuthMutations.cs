using AutoMapper;
using LogiCore.Application.Common.Interfaces.Security;
using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LogiCore.Api.GraphQL;

public partial class Mutation
{
	public async Task<UserDto> Register(string firstName, string lastName, string email, string password, IEnumerable<string>? roles, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new RegisterUserCommand
		{
			FirstName = firstName,
			LastName = lastName,
			Email = email,
			Password = password,
			Roles = roles
		}));
	}

	public async Task<AuthResponseDto> Login(string email, string password, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor, [Service] IWebHostEnvironment environment)
	{
		var result = await mediator.Send(new LoginUserCommand { Email = email, Password = password });
		if (!result.IsSuccess) throw new InvalidOperationException(result.Error ?? "Login failed");

		if (result.Value?.RefreshToken is not null)
		{
			accessor.HttpContext?.Response.Cookies.Append("refreshToken", result.Value.RefreshToken, new CookieOptions
			{
				HttpOnly = true,
				Secure = environment.IsProduction(),
				SameSite = SameSiteMode.Lax,
				Expires = DateTime.UtcNow.AddDays(30)
			});
		}

		return result.Value!;
	}

	public async Task<AuthResponseDto> Refresh(string? refreshToken, [Service] IRefreshTokenService refreshTokenService, [Service] IJwtProvider jwtProvider, [Service] UserManager<LogiCore.Domain.Entities.ApplicationUser> userManager, [Service] IMapper mapper, [Service] IHttpContextAccessor accessor)
	{
		var token = accessor.HttpContext?.Request.Cookies["refreshToken"] ?? refreshToken;
		if (string.IsNullOrWhiteSpace(token)) throw new InvalidOperationException("Missing refresh token");

		var user = await refreshTokenService.ValidateRefreshTokenAsync(token);
		if (user == null) throw new InvalidOperationException("Invalid refresh token");

		var roles = await userManager.GetRolesAsync(user);
		var additionalClaims = roles.Select(role => new KeyValuePair<string, string>(System.Security.Claims.ClaimTypes.Role, role));
		var newAccessToken = jwtProvider.CreateToken(user.Id, user.Email ?? string.Empty, additionalClaims);

		await refreshTokenService.RevokeRefreshTokenAsync(token);
		var newRefreshToken = await refreshTokenService.CreateRefreshTokenAsync(user.Id);

		accessor.HttpContext?.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
		{
			HttpOnly = true,
			Secure = accessor.HttpContext?.Request.IsHttps == true,
			SameSite = SameSiteMode.Lax,
			Expires = DateTime.UtcNow.AddDays(30)
		});

		var userDto = mapper.Map<UserDto>(user);
		return new AuthResponseDto(newAccessToken, userDto, newRefreshToken);
	}
}
