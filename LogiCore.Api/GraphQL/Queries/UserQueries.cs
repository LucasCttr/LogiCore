using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Features.User.GetAll;
using LogiCore.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LogiCore.Api.GraphQL;

public partial class Query
{
	public async Task<PagedResult<UserDto>> GetUsers([Service] IMediator mediator, [Service] IHttpContextAccessor accessor, int page = 1, int pageSize = 15)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new GetAllUsersQuery(page, pageSize)));
	}

	public async Task<UserDto?> GetUser(Guid id, [Service] UserManager<ApplicationUser> userManager, [Service] IMapper mapper, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");

		var user = await userManager.FindByIdAsync(id.ToString());
		if (user == null) return null;

		var roles = await userManager.GetRolesAsync(user);
		var dto = mapper.Map<UserDto>(user);
		return dto with { Roles = roles };
	}
}
