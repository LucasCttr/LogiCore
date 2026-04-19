using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using LogiCore.Application.Features.Shipment;
using LogiCore.Application.Features.Shipment.CreateShipment;
using LogiCore.Application.Features.Shipment.GetById;
using LogiCore.Application.Features.Shipment.GetAll;
using LogiCore.Application.Features.Shipment.AddPackageToShipment;
using LogiCore.Application.Features.Shipment.DispatchShipment;
using LogiCore.Application.Features.Shipment.AssignDriver;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LogiCore.Application.Features.Driver.GetByUser;
using LogiCore.Application.Features.Shipment.GetByDriver;
using LogiCore.Application.Features.Shipment.ArriveShipment;
using LogiCore.Application.Features.Shipment.CompleteShipment;
using LogiCore.Application.Features.Shipment.CancelShipment;
using LogiCore.Application.Features.Shipment.GetPaged;
using LogiCore.Application.Features.Shipment.AddPackagesToShipment;
using LogiCore.Application.Features.Packages;
using LogiCore.Application.Features.Shipment.StartShipment;
using LogiCore.Application.Features.Shipment.FinalizeShipment;

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

    // GET: api/shipments (Admin only) - supports paging, sorting and filtering via query params
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<Result<PagedResultDto<ShipmentDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? sortBy = null, [FromQuery] string? sortDir = null, [FromQuery] string? status = null, [FromQuery] string? q = null)
    {
        var result = await _mediator.Send(new GetShipmentsQuery(page, pageSize, sortBy, sortDir, status, q));
        return result;
    }

    // GET: api/shipments/me (Driver only) - shipments assigned to current driver
    [Authorize(Roles = "Driver")]
    [HttpGet("me")]
    public async Task<ActionResult<Result<IEnumerable<ShipmentDto>>>> GetMyShipments()
    {
        var currentUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId)) return Forbid();

        // get driver id for current user
        var driverResult = await _mediator.Send(new GetDriverByUserQuery(currentUserId));
        if (driverResult == null || !driverResult.IsSuccess) return Result<IEnumerable<ShipmentDto>>.Failure("Driver profile not found.", ErrorType.NotFound);

        var driverId = driverResult.Value!.Id;
        var shipmentsResult = await _mediator.Send(new GetShipmentsByDriverQuery(driverId));
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
        var currentUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId)) return Forbid();

        var driverResult = await _mediator.Send(new GetDriverByUserQuery(currentUserId));
        if (driverResult == null || !driverResult.IsSuccess || driverResult.Value == null) return Forbid();

        var driver = driverResult.Value;
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

    // POST: api/shipments/{id}/add-packages (Admin only) - add multiple packages at once
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/add-packages")]
    public async Task<ActionResult<Result<bool>>> AddPackages(Guid id, [FromBody] List<Guid> packageIds)
    {
        var command = new AddPackagesToShipmentCommand 
        { 
            ShipmentId = id, 
            PackageIds = packageIds 
        };
        var result = await _mediator.Send(command);
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

    // POST: api/shipments/{id}/start (Driver only)
    // Marks shipment as active (Dispatched)
    // Driver can have only one active shipment at a time
    // Packages are scanned separately using scanner endpoint
    [Authorize(Roles = "Driver")]
    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<Result<bool>>> Start(Guid id)
    {
        // Ensure only assigned driver can start their shipments
        var getResult = await _mediator.Send(new GetShipmentByIdQuery(id));
        if (getResult == null) return NotFound();
        if (!getResult.IsSuccess) return BadRequest(getResult);

        var shipment = getResult.Value!;
        var currentUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId)) return Forbid();

        var driverResult = await _mediator.Send(new GetDriverByUserQuery(currentUserId));
        if (driverResult == null || !driverResult.IsSuccess) return Forbid();
        var driver = driverResult.Value!;

        if (shipment.DriverId == null || driver.Id != shipment.DriverId.Value) return Forbid();

        var result = await _mediator.Send(new StartShipmentCommand { ShipmentId = id });
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

    // POST: api/shipments/{id}/arrive (Driver only)
    [Authorize(Roles = "Driver")]
    [HttpPost("{id:guid}/arrive")]
    public async Task<ActionResult<Result<bool>>> Arrive(Guid id)
    {
        // ensure only assigned driver can mark arrival
        var getResult = await _mediator.Send(new GetShipmentByIdQuery(id));
        if (getResult == null) return NotFound();
        if (!getResult.IsSuccess) return BadRequest(getResult);

        var shipment = getResult.Value!;
        var currentUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId)) return Forbid();

        var driverResult = await _mediator.Send(new GetDriverByUserQuery(currentUserId));
        if (driverResult == null || !driverResult.IsSuccess) return Forbid();
        var driver = driverResult.Value!;

        if (shipment.DriverId == null || driver.Id != shipment.DriverId.Value) return Forbid();

        var result = await _mediator.Send(new ArriveShipmentCommand { ShipmentId = id });
        return result;
    }



    // POST: api/shipments/{id}/complete (Driver only)
    [Authorize(Roles = "Driver")]
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<Result<bool>>> Complete(Guid id)
    {
        // ensure only assigned driver can complete
        var getResult = await _mediator.Send(new GetShipmentByIdQuery(id));
        if (getResult == null) return NotFound();
        if (!getResult.IsSuccess) return BadRequest(getResult);

        var shipment = getResult.Value!;
        var currentUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId)) return Forbid();

        var driverResult = await _mediator.Send(new GetDriverByUserQuery(currentUserId));
        if (driverResult == null || !driverResult.IsSuccess) return Forbid();
        var driver = driverResult.Value!;

        if (shipment.DriverId == null || driver.Id != shipment.DriverId.Value) return Forbid();

        var result = await _mediator.Send(new CompleteShipmentCommand { ShipmentId = id });
        return result;
    }

    // POST: api/shipments/{id}/finalize (Driver only)
    // Finalizes the shipment based on its type:
    // - Pickup: marks as Delivered, packages keep their status
    // - Transfer: marks as Delivered, updates packages to AtDepot at destination
    // - LastMile: marks as Delivered, packages keep their status
    [Authorize(Roles = "Driver")]
    [HttpPost("{id:guid}/finalize")]
    public async Task<ActionResult<Result<bool>>> Finalize(Guid id)
    {
        // ensure only assigned driver can finalize
        var getResult = await _mediator.Send(new GetShipmentByIdQuery(id));
        if (getResult == null) return NotFound();
        if (!getResult.IsSuccess) return BadRequest(getResult);

        var shipment = getResult.Value!;
        var currentUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId)) return Forbid();

        var driverResult = await _mediator.Send(new GetDriverByUserQuery(currentUserId));
        if (driverResult == null || !driverResult.IsSuccess) return Forbid();
        var driver = driverResult.Value!;

        if (shipment.DriverId == null || driver.Id != shipment.DriverId.Value) return Forbid();

        var result = await _mediator.Send(new FinalizeShipmentCommand(id));
        return result;
    }

    // POST: api/shipments/{id}/cancel (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<Result<bool>>> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelShipmentCommand { ShipmentId = id });
        return result;
    }
}
