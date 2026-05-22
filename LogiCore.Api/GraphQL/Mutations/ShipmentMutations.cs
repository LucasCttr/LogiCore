using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Shipment.AddPackageToShipment;
using LogiCore.Application.Features.Shipment.AddPackagesToShipment;
using LogiCore.Application.Features.Shipment.ArriveShipment;
using LogiCore.Application.Features.Shipment.AssignDriver;
using LogiCore.Application.Features.Shipment.CancelShipment;
using LogiCore.Application.Features.Shipment.CompleteShipment;
using LogiCore.Application.Features.Shipment.CreateShipment;
using LogiCore.Application.Features.Shipment.DispatchShipment;
using LogiCore.Application.Features.Shipment.FinalizeShipment;
using LogiCore.Application.Features.Shipment.StartShipment;
using MediatR;

namespace LogiCore.Api.GraphQL;

public partial class Mutation
{
	public async Task<ShipmentDto> CreateShipment(CreateShipmentCommand request, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(request));
	}

	public async Task<ShipmentDto> AddPackageToShipment(Guid shipmentId, Guid packageId, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new AddPackageToShipmentCommand { ShipmentId = shipmentId, PackageId = packageId }));
	}

	public async Task<bool> AddPackagesToShipment(Guid shipmentId, IEnumerable<Guid> packageIds, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new AddPackagesToShipmentCommand { ShipmentId = shipmentId, PackageIds = packageIds.ToList() }));
	}

	public async Task<bool> DispatchShipment(Guid shipmentId, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new DispatchShipmentCommand { ShipmentId = shipmentId }));
	}

	public async Task<bool> StartShipment(Guid shipmentId, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		await GraphQLHelpers.VerifyDriverOwnsShipment(shipmentId, mediator, accessor);
		return GraphQLHelpers.Unwrap(await mediator.Send(new StartShipmentCommand { ShipmentId = shipmentId }));
	}

	public async Task<bool> AssignDriverToShipment(Guid shipmentId, Guid driverId, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new AssignDriverToShipmentCommand { ShipmentId = shipmentId, DriverId = driverId }));
	}

	public async Task<bool> ArriveShipment(Guid shipmentId, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		await GraphQLHelpers.VerifyDriverOwnsShipment(shipmentId, mediator, accessor);
		return GraphQLHelpers.Unwrap(await mediator.Send(new ArriveShipmentCommand { ShipmentId = shipmentId }));
	}

	public async Task<bool> CompleteShipment(Guid shipmentId, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		await GraphQLHelpers.VerifyDriverOwnsShipment(shipmentId, mediator, accessor);
		return GraphQLHelpers.Unwrap(await mediator.Send(new CompleteShipmentCommand { ShipmentId = shipmentId }));
	}

	public async Task<bool> FinalizeShipment(Guid shipmentId, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		await GraphQLHelpers.VerifyDriverOwnsShipment(shipmentId, mediator, accessor);
		return GraphQLHelpers.Unwrap(await mediator.Send(new FinalizeShipmentCommand(shipmentId)));
	}

	public async Task<bool> CancelShipment(Guid shipmentId, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new CancelShipmentCommand { ShipmentId = shipmentId }));
	}
}
