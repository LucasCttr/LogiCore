using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using MediatR;
using LogiCore.Application.Features.Driver.Register;
using LogiCore.Application.Features.Driver.GetAll;
using LogiCore.Application.Features.Driver.GetById;
using LogiCore.Application.Features.Driver.UpdateStatus;
using LogiCore.Application.Common.Models;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DriversController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriversController(IMediator mediator)
    {
        _mediator = mediator;
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
}
