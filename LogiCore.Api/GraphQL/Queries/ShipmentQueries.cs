using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Driver.GetByUser;
using LogiCore.Application.Features.Shipment.GetByDriver;
using LogiCore.Application.Features.Shipment.GetById;
using LogiCore.Application.Features.Shipment.GetPaged;
using MediatR;

namespace LogiCore.Api.GraphQL;

public partial class Query
{
	public async Task<PagedResultDto<ShipmentDto>> GetShipments([Service] IMediator mediator, [Service] IHttpContextAccessor accessor, int page = 1, int pageSize = 10, string? sortBy = null, string? sortDir = null, string? status = null, string? q = null)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new GetShipmentsQuery(page, pageSize, sortBy, sortDir, status, q)));
	}

	public async Task<IEnumerable<ShipmentDto>> GetMyShipments([Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Driver");

		var currentUserId = GraphQLHelpers.GetCurrentUserId(accessor);
		if (string.IsNullOrWhiteSpace(currentUserId)) throw new InvalidOperationException("Unauthorized");

		var driverResult = await mediator.Send(new GetDriverByUserQuery(currentUserId));
		if (driverResult == null || !driverResult.IsSuccess || driverResult.Value == null) throw new InvalidOperationException("Driver profile not found.");

		return GraphQLHelpers.Unwrap(await mediator.Send(new GetShipmentsByDriverQuery(driverResult.Value.Id)));
	}

	public async Task<ShipmentDto?> GetShipment(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireAuthenticated(accessor);
		var result = await mediator.Send(new GetShipmentByIdQuery(id));
		if (result == null || !result.IsSuccess) return null;

		if (GraphQLHelpers.IsInRole(accessor, "Admin")) return result.Value;

		var currentUserId = GraphQLHelpers.GetCurrentUserId(accessor);
		if (string.IsNullOrWhiteSpace(currentUserId)) return null;

		var driverResult = await mediator.Send(new GetDriverByUserQuery(currentUserId));
		if (driverResult == null || !driverResult.IsSuccess || driverResult.Value == null) return null;

		var shipment = result.Value;
		return shipment?.DriverId != null && shipment.DriverId.Value == driverResult.Value.Id ? shipment : null;
	}
}
