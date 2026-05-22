using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Location.GetAll;
using MediatR;

namespace LogiCore.Api.GraphQL;

public partial class Query
{
	public async Task<IEnumerable<LocationDto>> GetLocations([Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new GetAllLocationsQuery()));
	}
}
