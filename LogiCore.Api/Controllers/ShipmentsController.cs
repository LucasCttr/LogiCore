using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using LogiCore.Application.Features.Shipment.CreateShipment;
using LogiCore.Application.Features.Shipment.GetById;
using LogiCore.Application.Features.Shipment.GetAll;
using LogiCore.Application.Features.Shipment.AddPackageToShipment;
using LogiCore.Application.Features.Shipment.DispatchShipment;
using LogiCore.Application.Features.Shipment.AssignDriver;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShipmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShipmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST: api/shipments (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Result<ShipmentDto>>> Create([FromBody] CreateShipmentCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }

    // GET: api/shipments (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<ShipmentDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllShipmentsQuery());
        return result;
    }

    // GET: api/shipments/me (Driver only) - shipments assigned to current driver
    [Authorize(Roles = "Driver")]
    [HttpGet("me")]
    public async Task<ActionResult<Result<IEnumerable<ShipmentDto>>>> GetMyShipments()
    {
        var currentUserId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId)) return Forbid();

        // get driver id for current user
        var driverResult = await _mediator.Send(new LogiCore.Application.Features.Driver.GetByUser.GetDriverByUserQuery(currentUserId));
        if (driverResult == null || !driverResult.IsSuccess) return Result<IEnumerable<ShipmentDto>>.Failure("Driver profile not found.", LogiCore.Application.Common.Models.ErrorType.NotFound);

        var driverId = driverResult.Value!.Id;
        var shipmentsResult = await _mediator.Send(new LogiCore.Application.Features.Shipment.GetByDriver.GetShipmentsByDriverQuery(driverId));
        return shipmentsResult;
    }

    // GET: api/shipments/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<ShipmentDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetShipmentByIdQuery(id));
        if (result == null) return NotFound();
        if (!result.IsSuccess) return result;

        var shipment = result.Value;

        // allow admins
        if (User.IsInRole("Admin")) return result;

        // otherwise allow only the driver assigned to this shipment
        var currentUserId = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId)) return Forbid();

        var driverResult = await _mediator.Send(new Application.Features.Driver.GetByUser.GetDriverByUserQuery(currentUserId));
        if (driverResult == null || !driverResult.IsSuccess) return Forbid();

        var driver = driverResult.Value!;
        if (shipment.DriverId == null || driver.Id != shipment.DriverId.Value) return Forbid();

        return result;
    }

    // POST: api/shipments/{id}/packages
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/packages")]
    public async Task<ActionResult<Result<ShipmentDto>>> AddPackage(Guid id, [FromBody] AddPackageToShipmentCommand request)
    {
        // ensure route id is used
        request.ShipmentId = id;
        var result = await _mediator.Send(request);
        return result;
    }

    // POST: api/shipments/{id}/dispatch (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/dispatch")]
    public async Task<ActionResult<Result<bool>>> Dispatch(Guid id)
    {
        var result = await _mediator.Send(new DispatchShipmentCommand { ShipmentId = id });
        return result;
    }

    // POST: api/shipments/{id}/assign-driver
    [HttpPost("{id:guid}/assign-driver")]
    public async Task<ActionResult<Result<bool>>> AssignDriver(Guid id, [FromBody] AssignDriverToShipmentCommand request)
    {
        request.ShipmentId = id;
        var result = await _mediator.Send(request);
        return result;
    }
}
