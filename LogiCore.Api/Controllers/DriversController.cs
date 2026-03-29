using Microsoft.AspNetCore.Mvc;
using MediatR;
using LogiCore.Application.Features.Driver.Register;
using LogiCore.Application.Features.Driver.GetAll;
using LogiCore.Application.Features.Driver.GetById;
using LogiCore.Application.Features.Driver.UpdateStatus;
using LogiCore.Application.Common.Models;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DriversController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriversController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST: api/drivers/register
    [HttpPost("register")]
    public async Task<ActionResult<Result<LogiCore.Application.DTOs.DriverDto>>> Register([FromBody] RegisterDriverCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }

    // GET: api/drivers
    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<LogiCore.Application.DTOs.DriverDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllDriversQuery());
        return result;
    }

    // GET: api/drivers/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<LogiCore.Application.DTOs.DriverDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetDriverByIdQuery(id));
        return result;
    }

    // PUT: api/drivers/{id}/status
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<Result<LogiCore.Application.DTOs.DriverDto>>> UpdateStatus(Guid id, [FromBody] UpdateDriverStatusCommand request)
    {
        request.DriverId = id;
        var result = await _mediator.Send(request);
        return result;
    }
}
