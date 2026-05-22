using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Driver;
using LogiCore.Application.Features.Driver.Register;
using LogiCore.Application.Features.Driver.Update;
using LogiCore.Application.Features.Driver.UpdateStatus;
using MediatR;

namespace LogiCore.Api.GraphQL;

public partial class Mutation
{
	public async Task<DriverDto> RegisterDriver(RegisterDriverCommand request, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(request));
	}

	public async Task<DriverDto> UpdateDriver(Guid id, UpdateDriverCommand request, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");

		request.DriverId = id;
		return GraphQLHelpers.Unwrap(await mediator.Send(request));
	}

	public async Task<DriverDto> UpdateDriverStatus(Guid id, UpdateDriverStatusCommand request, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");

		request.DriverId = id;
		return GraphQLHelpers.Unwrap(await mediator.Send(request));
	}

	public async Task<DriverDto> AssignVehicleToDriver(Guid id, Guid? vehicleId, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new AssignVehicleToDriverCommand { DriverId = id, VehicleId = vehicleId }));
	}
}
