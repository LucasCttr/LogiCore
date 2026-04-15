using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using MediatR;
using LogiCore.Application.Features.Driver.Register;
using LogiCore.Application.Features.Driver.GetAll;
using LogiCore.Application.Features.Driver.GetById;
using LogiCore.Application.Features.Driver.UpdateStatus;
using LogiCore.Application.Features.Driver.Update;
using LogiCore.Application.Features.Driver;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Repositories;
using LogiCore.Api.Models;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDriverRepository _driverRepository;
    private readonly IDriverDetailsRepository _driverDetailsRepository;

    public DriversController(
        IMediator mediator,
        IDriverRepository driverRepository,
        IDriverDetailsRepository driverDetailsRepository)
    {
        _mediator = mediator;
        _driverRepository = driverRepository;
        _driverDetailsRepository = driverDetailsRepository;
    }

    // POST: api/drivers/register (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpPost("register")]
    public async Task<ActionResult<Result<LogiCore.Application.DTOs.DriverDto>>> Register([FromBody] RegisterDriverCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }

    // GET: api/drivers (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<LogiCore.Application.DTOs.DriverDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllDriversQuery());
        return result;
    }

    // GET: api/drivers/available (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpGet("available")]
    public async Task<ActionResult<Result<IEnumerable<LogiCore.Application.DTOs.DriverDto>>>> GetAvailable()
    {
        var result = await _mediator.Send(new LogiCore.Application.Features.Driver.GetAvailable.GetAvailableDriversQuery());
        return result;
    }

    // GET: api/drivers/details (Admin only - new endpoint for drivers from DriverDetails table)
    [Authorize(Roles = "Admin")]
    [HttpGet("details")]
    public async Task<ActionResult<Result<LogiCore.Application.Common.Models.PagedResult<LogiCore.Application.DTOs.DriverDetailsWithUserDto>>>> GetAllDetails([FromQuery] int page = 1, [FromQuery] int pageSize = 15, [FromQuery] string? search = null, [FromQuery] bool? isActive = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 15;
        if (pageSize > 100) pageSize = 100;

        var query = new LogiCore.Application.Features.Driver.GetAll.GetAllDriverDetailsQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            SearchTerm = search,
            IsActive = isActive
        };

        var result = await _mediator.Send(query);
        return result;
    }

    // GET: api/drivers/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<LogiCore.Application.DTOs.DriverDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDriverByIdQuery(id));
        if (result == null) return NotFound();
        if (!result.IsSuccess) return result;

        var driver = result.Value;

        // allow admins
        if (User.IsInRole("Admin")) return result;

        // otherwise allow only the driver who owns this profile
        var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId) || !string.Equals(driver.ApplicationUserId ?? string.Empty, currentUserId, StringComparison.Ordinal))
            return Forbid();

        return result;
    }

    // GET: api/drivers/me
    [Authorize(Roles = "Driver")]
    [HttpGet("me")]
    public async Task<ActionResult<Result<LogiCore.Application.DTOs.DriverDto>>> GetMyProfile()
    {
        var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(currentUserId)) return Forbid();

        var result = await _mediator.Send(new LogiCore.Application.Features.Driver.GetByUser.GetDriverByUserQuery(currentUserId));
        return result;
    }

    // PUT: api/drivers/{id}/status
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<Result<LogiCore.Application.DTOs.DriverDto>>> UpdateStatus(Guid id, [FromBody] UpdateDriverStatusCommand request)
    {
        // authorize: admins can update any driver; drivers can only update their own status
        var getResult = await _mediator.Send(new GetDriverByIdQuery(id));
        if (getResult == null) return NotFound();
        if (!getResult.IsSuccess) return getResult;

        var driver = getResult.Value!;
        var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && (string.IsNullOrEmpty(currentUserId) || !string.Equals(driver.ApplicationUserId ?? string.Empty, currentUserId, StringComparison.Ordinal)))
            return Forbid();

        request.DriverId = id;
        var result = await _mediator.Send(request);
        return result;
    }

    // PUT: api/drivers/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<LogiCore.Application.DTOs.DriverDto>>> UpdateDriver(Guid id, [FromBody] UpdateDriverCommand request)
    {
        // authorize: admins can update any driver; drivers can only update their own profile
        var getResult = await _mediator.Send(new GetDriverByIdQuery(id));
        if (getResult == null) return NotFound();
        if (!getResult.IsSuccess) return getResult;

        var driver = getResult.Value!;
        var currentUserId = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && (string.IsNullOrEmpty(currentUserId) || !string.Equals(driver.ApplicationUserId ?? string.Empty, currentUserId, StringComparison.Ordinal)))
            return Forbid();

        request.DriverId = id;
        var result = await _mediator.Send(request);
        return result;
    }

    // PUT: api/drivers/{id}/assign-vehicle
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/assign-vehicle")]
    public async Task<ActionResult<Result<LogiCore.Application.DTOs.DriverDto>>> AssignVehicle(Guid id, [FromBody] AssignVehicleRequest request)
    {
        // Try to use the provided ID as Driver ID
        Guid driverId = id;
        
        // If Driver not found, try to find it as DriverDetails ID
        var driver = await _driverRepository.GetByIdAsync(id);
        if (driver is null)
        {
            // Try to get as DriverDetails
            var driverDetails = await _driverDetailsRepository.GetByIdAsync(id);
            if (driverDetails is not null)
            {
                // Try to find Driver by userId
                driver = await _driverRepository.GetByApplicationUserIdAsync(driverDetails.UserId);
                if (driver is not null)
                {
                    driverId = driver.Id;
                }
                else
                {
                    // Driver doesn't exist, we'll send the userId to the command handler
                    // For now, use the DriverDetails ID - the handler will create Driver if needed
                    driverId = id;
                }
            }
        }
        
        var command = new AssignVehicleToDriverCommand { DriverId = driverId, VehicleId = request.VehicleId };
        var result = await _mediator.Send(command);
        return result;
    }
}
