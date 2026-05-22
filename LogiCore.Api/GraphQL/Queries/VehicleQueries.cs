using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Vehicle.GetAllVehicles;
using LogiCore.Application.Features.Vehicle.GetById;
using MediatR;

namespace LogiCore.Api.GraphQL;

public partial class Query
{
	public async Task<IEnumerable<VehicleDto>> GetVehicles([Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireAuthenticated(accessor);
		return GraphQLHelpers.Unwrap(await mediator.Send(new GetAllVehiclesQuery()));
	}

	public async Task<VehicleDto?> GetVehicle(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireAuthenticated(accessor);
		var result = await mediator.Send(new GetVehicleByIdQuery(id));
		return result?.IsSuccess == true ? result.Value : null;
	}

	public async Task<IEnumerable<VehicleDto>> GetAvailableVehicles([Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireAuthenticated(accessor);

		var result = GraphQLHelpers.Unwrap(await mediator.Send(new GetAllVehiclesQuery()));
		return result.Where(vehicle => vehicle.IsActive);
	}
}
