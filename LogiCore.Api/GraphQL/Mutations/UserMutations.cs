using AutoMapper;
using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LogiCore.Api.GraphQL;

public partial class Mutation
{
	public async Task<UserDto> CreateUser(string firstName, string lastName, string email, string password, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new RegisterUserCommand
		{
			FirstName = firstName,
			LastName = lastName,
			Email = email,
			Password = password
		}));
	}

	public async Task<UserDto> UpdateUser(Guid id, string? firstName, string? lastName, string? email, IEnumerable<string>? roles, [Service] UserManager<LogiCore.Domain.Entities.ApplicationUser> userManager, [Service] IMapper mapper, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");

		var user = await userManager.FindByIdAsync(id.ToString());
		if (user == null) throw new InvalidOperationException("Not found");

		if (firstName != null) user.FirstName = firstName;
		if (lastName != null) user.LastName = lastName;
		if (email != null)
		{
			user.Email = email;
			user.UserName = email;
		}

		var updateResult = await userManager.UpdateAsync(user);
		if (!updateResult.Succeeded) throw new InvalidOperationException(updateResult.Errors.FirstOrDefault()?.Description ?? "Failed to update user");

		if (roles != null)
		{
			var existingRoles = await userManager.GetRolesAsync(user);
			if (existingRoles.Any())
			{
				var removeResult = await userManager.RemoveFromRolesAsync(user, existingRoles);
				if (!removeResult.Succeeded) throw new InvalidOperationException(removeResult.Errors.FirstOrDefault()?.Description ?? "Failed to update roles");
			}

			if (roles.Any())
			{
				var addResult = await userManager.AddToRolesAsync(user, roles);
				if (!addResult.Succeeded) throw new InvalidOperationException(addResult.Errors.FirstOrDefault()?.Description ?? "Failed to update roles");
			}
		}

		var refreshedUser = await userManager.FindByIdAsync(id.ToString());
		if (refreshedUser == null) throw new InvalidOperationException("Not found");
		var refreshedRoles = await userManager.GetRolesAsync(refreshedUser);
		return mapper.Map<UserDto>(refreshedUser) with { Roles = refreshedRoles };
	}

	public async Task<bool> ToggleUserStatus(Guid id, bool isActive, [Service] UserManager<LogiCore.Domain.Entities.ApplicationUser> userManager, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");

		var user = await userManager.FindByIdAsync(id.ToString());
		if (user == null) throw new InvalidOperationException("Not found");

		var lockoutEnd = isActive ? (DateTimeOffset?)null : DateTimeOffset.UtcNow.AddYears(100);
		user.LockoutEnd = lockoutEnd;
		var result = await userManager.UpdateAsync(user);
		if (!result.Succeeded) throw new InvalidOperationException(result.Errors.FirstOrDefault()?.Description ?? "Failed to update user status");

		return true;
	}
}
