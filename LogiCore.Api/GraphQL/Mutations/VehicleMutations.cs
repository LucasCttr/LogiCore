using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Vehicle.CreateVehicle;
using LogiCore.Application.Features.Vehicle.DeleteVehicle;
using LogiCore.Application.Features.Vehicle.UpdateStatus;
using LogiCore.Application.Features.Vehicle.UpdateVehicle;
using MediatR;

namespace LogiCore.Api.GraphQL;

public partial class Mutation
{
	public async Task<VehicleDto> CreateVehicle(CreateVehicleDto request, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new CreateVehicleCommand(request.Plate, request.Make, request.Model, request.MaxWeightCapacity, request.MaxVolumeCapacity)));
	}

	public async Task<VehicleDto> UpdateVehicle(Guid id, UpdateVehicleDto request, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new UpdateVehicleCommand(id, request.Plate, request.Make, request.Model, request.MaxWeightCapacity, request.MaxVolumeCapacity, request.IsActive)));
	}

	public async Task<bool> DeleteVehicle(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new DeleteVehicleCommand(id)));
	}

	public async Task<VehicleDto> UpdateVehicleStatus(Guid id, UpdateVehicleStatusDto request, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new UpdateVehicleStatusCommand(id, request.Status)));
	}
}
