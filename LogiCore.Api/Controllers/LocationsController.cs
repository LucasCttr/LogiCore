using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Location.GetAll;
using LogiCore.Application.Features.Location.CreateLocation;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public LocationsController(IMediator mediator) => _mediator = mediator;

    // GET: api/locations (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<LocationDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllLocationsQuery());
        return result;
    }

    // POST: api/locations (Admin only)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Result<LocationDto>>> Create([FromBody] CreateLocationCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }
}
