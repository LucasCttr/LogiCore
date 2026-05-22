using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Driver.GetAll;
using LogiCore.Application.Features.Driver.GetById;
using LogiCore.Application.Features.Driver.GetByUser;
using MediatR;

namespace LogiCore.Api.GraphQL;

public partial class Query
{
	public async Task<IEnumerable<DriverDto>> GetDrivers([Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new GetAllDriversQuery()));
	}

	public async Task<IEnumerable<DriverDto>> GetAvailableDrivers([Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new LogiCore.Application.Features.Driver.GetAvailable.GetAvailableDriversQuery()));
	}

	public async Task<PagedResult<DriverDetailsWithUserDto>> GetDriverDetails([Service] IMediator mediator, [Service] IHttpContextAccessor accessor, int page = 1, int pageSize = 15, string? search = null, bool? isActive = null)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");

		var query = new GetAllDriverDetailsQuery
		{
			PageNumber = page,
			PageSize = pageSize,
			SearchTerm = search,
			IsActive = isActive
		};

		return GraphQLHelpers.Unwrap(await mediator.Send(query));
	}

	public async Task<DriverDto?> GetDriver(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireAuthenticated(accessor);
		var result = await mediator.Send(new GetDriverByIdQuery(id));
		if (result == null || !result.IsSuccess) return null;

		if (GraphQLHelpers.IsInRole(accessor, "Admin")) return result.Value;

		var currentUserId = GraphQLHelpers.GetCurrentUserId(accessor);
		return !string.IsNullOrWhiteSpace(currentUserId) && string.Equals(result.Value?.ApplicationUserId ?? string.Empty, currentUserId, StringComparison.Ordinal)
			? result.Value
			: null;
	}

	public async Task<DriverDto?> GetMyDriverProfile([Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Driver");

		var currentUserId = GraphQLHelpers.GetCurrentUserId(accessor);
		if (string.IsNullOrWhiteSpace(currentUserId)) throw new InvalidOperationException("Unauthorized");

		return GraphQLHelpers.Unwrap(await mediator.Send(new GetDriverByUserQuery(currentUserId)));
	}
}
