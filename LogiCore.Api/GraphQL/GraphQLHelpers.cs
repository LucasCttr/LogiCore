using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Packages;
using LogiCore.Application.Features.Driver.GetByUser;
using LogiCore.Application.Features.Shipment.GetById;
using MediatR;

namespace LogiCore.Api.GraphQL;

internal static class GraphQLHelpers
{
	internal static bool IsAuthenticated(IHttpContextAccessor accessor)
		=> accessor.HttpContext?.User.Identity?.IsAuthenticated == true;

	internal static bool IsInRole(IHttpContextAccessor accessor, string role)
		=> accessor.HttpContext?.User.IsInRole(role) == true;

	internal static string? GetCurrentUserId(IHttpContextAccessor accessor)
	{
		var user = accessor.HttpContext?.User;
		return user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
			?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
	}

	internal static T Unwrap<T>(LogiCore.Application.Common.Models.Result<T>? result, string defaultError = "Operation failed")
	{
		if (result == null) throw new InvalidOperationException(defaultError);
		if (!result.IsSuccess) throw new InvalidOperationException(result.Error ?? defaultError);
		return result.Value!;
	}

	internal static void RequireAuthenticated(IHttpContextAccessor accessor)
	{
		if (!IsAuthenticated(accessor)) throw new InvalidOperationException("Unauthorized");
	}

	internal static void RequireRole(IHttpContextAccessor accessor, string role)
	{
		if (!IsInRole(accessor, role)) throw new InvalidOperationException("Unauthorized");
	}

	internal static async Task<ShipmentDto> VerifyDriverOwnsShipment(Guid shipmentId, IMediator mediator, IHttpContextAccessor accessor)
	{
		RequireRole(accessor, "Driver");

		var shipmentResult = await mediator.Send(new GetShipmentByIdQuery(shipmentId));
		if (shipmentResult == null || !shipmentResult.IsSuccess || shipmentResult.Value == null) throw new InvalidOperationException("Not found");

		var currentUserId = GetCurrentUserId(accessor);
		if (string.IsNullOrWhiteSpace(currentUserId)) throw new InvalidOperationException("Unauthorized");

		var driverResult = await mediator.Send(new GetDriverByUserQuery(currentUserId));
		if (driverResult == null || !driverResult.IsSuccess || driverResult.Value == null) throw new InvalidOperationException("Forbidden");

		if (shipmentResult.Value.DriverId == null || shipmentResult.Value.DriverId.Value != driverResult.Value.Id)
			throw new InvalidOperationException("Forbidden");

		return shipmentResult.Value;
	}

	internal static async Task<PackageDetailDto> VerifyDriverOwnsPackage(Guid packageId, IMediator mediator, IHttpContextAccessor accessor)
	{
		RequireRole(accessor, "Driver");

		var packageResult = await mediator.Send(new GetPackageByIdQuery(packageId));
		if (packageResult == null || !packageResult.IsSuccess || packageResult.Value == null) throw new InvalidOperationException("Not found");
		if (packageResult.Value.CurrentShipment?.Id == null) throw new InvalidOperationException("Forbidden");

		await VerifyDriverOwnsShipment(packageResult.Value.CurrentShipment.Id, mediator, accessor);
		return packageResult.Value;
	}
}