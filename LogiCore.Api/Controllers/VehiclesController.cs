using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Models;
using AutoMapper;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VehiclesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/vehicles
    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<VehicleDto>>>> GetAll()
    {
        return await _mediator.Send(new LogiCore.Application.Features.Vehicle.GetAllVehicles.GetAllVehiclesQuery());
    }

    // GET: api/vehicles/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<VehicleDto>>> GetById(Guid id)
    {
        return await _mediator.Send(new LogiCore.Application.Features.Vehicle.GetById.GetVehicleByIdQuery(id));
    }

    // POST: api/vehicles
    [HttpPost]
    public async Task<ActionResult<Result<VehicleDto>>> Create([FromBody] CreateVehicleDto request)
    {
        return await _mediator.Send(new LogiCore.Application.Features.Vehicle.CreateVehicle.CreateVehicleCommand(request.Plate, request.MaxWeightCapacity, request.MaxVolumeCapacity));
    }

    // PUT: api/vehicles/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<VehicleDto>>> Update(Guid id, [FromBody] UpdateVehicleDto request)
    {
        return await _mediator.Send(new LogiCore.Application.Features.Vehicle.UpdateVehicle.UpdateVehicleCommand(id, request.Plate, request.MaxWeightCapacity, request.MaxVolumeCapacity, request.IsActive));
    }

    // DELETE: api/vehicles/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result<bool>>> Delete(Guid id)
    {
        return await _mediator.Send(new LogiCore.Application.Features.Vehicle.DeleteVehicle.DeleteVehicleCommand(id));
    }
}
