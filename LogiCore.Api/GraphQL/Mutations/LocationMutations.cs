using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Location.CreateLocation;
using MediatR;

namespace LogiCore.Api.GraphQL;

public partial class Mutation
{
	public async Task<LocationDto> CreateLocation(CreateLocationCommand request, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(request));
	}
}
