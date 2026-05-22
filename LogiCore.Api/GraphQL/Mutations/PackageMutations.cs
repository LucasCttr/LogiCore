using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Package.CollectPackage;
using LogiCore.Application.Features.Package.MarkAttemptFailed;
using LogiCore.Application.Features.Package.MarkPackageAsCollected;
using LogiCore.Application.Features.Package.MarkPackageAsDelivered;
using LogiCore.Application.Features.Package.MoveToDepot;
using LogiCore.Application.Features.Packages;
using LogiCore.Application.Services;
using MediatR;

namespace LogiCore.Api.GraphQL;

public partial class Mutation
{
	public async Task<PackageDto> CreatePackage(CreatePackageCommand request, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(request));
	}

	public async Task<PackageDto> UpdatePackage(Guid id, UpdatePackageCommand request, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		request = request with { Id = id };
		return GraphQLHelpers.Unwrap(await mediator.Send(request));
	}

	public async Task<PackageDto> DeliverPackage(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new DeliverPackageCommand(id)));
	}

	public async Task<PackageDto> CancelPackage(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new CancelPackageCommand(id)));
	}

	public async Task<bool> MovePackageToDepot(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new MovePackageToDepotCommand(id)));
	}

	public async Task<bool> MarkPackageAsDelivered(Guid id, decimal? latitude, decimal? longitude, string? deliveryNotes, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		await GraphQLHelpers.VerifyDriverOwnsPackage(id, mediator, accessor);
		return GraphQLHelpers.Unwrap(await mediator.Send(new MarkPackageAsDeliveredCommand
		{
			PackageId = id,
			Latitude = latitude,
			Longitude = longitude,
			DeliveryNotes = deliveryNotes
		}));
	}

	public async Task<bool> MarkPackageAsCollected(Guid id, string? collectionNotes, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		await GraphQLHelpers.VerifyDriverOwnsPackage(id, mediator, accessor);
		return GraphQLHelpers.Unwrap(await mediator.Send(new MarkPackageAsCollectedCommand { PackageId = id, CollectionNotes = collectionNotes }));
	}

	public async Task<bool> CollectPackage(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		await GraphQLHelpers.VerifyDriverOwnsPackage(id, mediator, accessor);
		return GraphQLHelpers.Unwrap(await mediator.Send(new CollectPackageCommand(id)));
	}

	public async Task<bool> MarkPackageAttemptFailed(Guid id, string? reason, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		await GraphQLHelpers.VerifyDriverOwnsPackage(id, mediator, accessor);
		return GraphQLHelpers.Unwrap(await mediator.Send(new MarkPackageAttemptFailedCommand { PackageId = id, Reason = reason }));
	}

	public async Task<bool> RecordSelectedAddress(string address, [Service] IAddressAutocompleteService service)
	{
		await service.RecordSelectionAsync(address);
		return true;
	}
}
