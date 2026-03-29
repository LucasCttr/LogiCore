using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Models;
using AutoMapper;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Api.Controllers;

using LogiCore.Application.Features.Vehicle;
using LogiCore.Application.Features.Vehicle.CreateVehicle;
using LogiCore.Application.Features.Vehicle.DeleteVehicle;
using LogiCore.Application.Features.Vehicle.GetAllVehicles;
using LogiCore.Application.Features.Vehicle.GetById;
using LogiCore.Application.Features.Vehicle.UpdateStatus;
using LogiCore.Application.Features.Vehicle.UpdateVehicle;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        return await _mediator.Send(new GetAllVehiclesQuery());
    }

    // GET: api/vehicles/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<VehicleDto>>> GetById(Guid id)
    {
        return await _mediator.Send(new GetVehicleByIdQuery(id));
    }

    // POST: api/vehicles
    [HttpPost]
    public async Task<ActionResult<Result<VehicleDto>>> Create([FromBody] CreateVehicleDto request)
    {
        return await _mediator.Send(new CreateVehicleCommand(request.Plate, request.MaxWeightCapacity, request.MaxVolumeCapacity));
    }

    // PUT: api/vehicles/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<VehicleDto>>> Update(Guid id, [FromBody] UpdateVehicleDto request)
    {
        return await _mediator.Send(new UpdateVehicleCommand(id, request.Plate, request.MaxWeightCapacity, request.MaxVolumeCapacity, request.IsActive));
    }

    // DELETE: api/vehicles/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result<bool>>> Delete(Guid id)
    {
        return await _mediator.Send(new DeleteVehicleCommand(id));
    }

    // PATCH: api/vehicles/{id}/status
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Result<VehicleDto>>> UpdateStatus(Guid id, [FromBody] UpdateVehicleStatusDto request)
    {
        return await _mediator.Send(new UpdateVehicleStatusCommand(id, request.Status));
    }
}
