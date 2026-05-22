using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Package.GetPackageForScanner;
using LogiCore.Application.Features.Package.GetPackagePublicHistory;
using LogiCore.Application.Features.Packages;
using LogiCore.Application.Services;
using MediatR;

namespace LogiCore.Api.GraphQL;

public partial class Query
{
	public async Task<PagedResponse<PackageDto>> GetPackages([Service] IMediator mediator, [Service] IHttpContextAccessor accessor, int page = 1, int pageSize = 20)
	{
		GraphQLHelpers.RequireRole(accessor, "Admin");
		return GraphQLHelpers.Unwrap(await mediator.Send(new GetAllPackagesQuery(page, pageSize)));
	}

	public async Task<PackageDetailDto?> GetPackage(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		if (GraphQLHelpers.IsInRole(accessor, "Admin"))
		{
			var adminResult = await mediator.Send(new GetPackageByIdQuery(id));
			return adminResult?.IsSuccess == true ? adminResult.Value : null;
		}

		if (GraphQLHelpers.IsInRole(accessor, "Driver"))
		{
			return await GraphQLHelpers.VerifyDriverOwnsPackage(id, mediator, accessor);
		}

		throw new InvalidOperationException("Unauthorized");
	}

	public async Task<PackagePublicHistoryDto?> GetPackageByTracking(string trackingNumber, [Service] IMediator mediator)
	{
		var result = await mediator.Send(new GetPackagePublicHistoryQuery(trackingNumber));
		return result?.IsSuccess == true ? result.Value : null;
	}

	public async Task<IEnumerable<PackageInternalHistoryDto>> GetPackageHistory(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		if (GraphQLHelpers.IsInRole(accessor, "Admin"))
		{
			return GraphQLHelpers.Unwrap(await mediator.Send(new LogiCore.Application.Features.Package.GetPackageHistory.GetPackageHistoryQuery(id)));
		}

		if (GraphQLHelpers.IsInRole(accessor, "Driver"))
		{
			await GraphQLHelpers.VerifyDriverOwnsPackage(id, mediator, accessor);
			return GraphQLHelpers.Unwrap(await mediator.Send(new LogiCore.Application.Features.Package.GetPackageHistory.GetPackageHistoryQuery(id)));
		}

		throw new InvalidOperationException("Unauthorized");
	}

	public async Task<PackageForScannerDto?> GetPackageForScannerByTracking(string trackingNumber, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		var result = await mediator.Send(new GetPackageForScannerByTrackingQuery(trackingNumber));
		if (result?.IsSuccess != true || result.Value == null) return null;

		if (GraphQLHelpers.IsInRole(accessor, "Admin")) return result.Value;
		if (GraphQLHelpers.IsInRole(accessor, "Driver"))
		{
			await GraphQLHelpers.VerifyDriverOwnsPackage(result.Value.Id, mediator, accessor);
			return result.Value;
		}

		throw new InvalidOperationException("Unauthorized");
	}

	public async Task<PackageForScannerDto?> GetPackageForScanner(Guid id, [Service] IMediator mediator, [Service] IHttpContextAccessor accessor)
	{
		var result = await mediator.Send(new GetPackageForScannerQuery(id));
		if (result?.IsSuccess != true || result.Value == null) return null;

		if (GraphQLHelpers.IsInRole(accessor, "Admin")) return result.Value;
		if (GraphQLHelpers.IsInRole(accessor, "Driver"))
		{
			await GraphQLHelpers.VerifyDriverOwnsPackage(id, mediator, accessor);
			return result.Value;
		}

		throw new InvalidOperationException("Unauthorized");
	}

	public async Task<IEnumerable<string>> GetAddressSuggestions([Service] IAddressAutocompleteService service, [Service] IHttpContextAccessor accessor, string q)
	{
		GraphQLHelpers.RequireAuthenticated(accessor);
		return await service.GetSuggestionsAsync(q, 5);
	}
}
