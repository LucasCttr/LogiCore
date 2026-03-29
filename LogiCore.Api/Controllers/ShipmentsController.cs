using Microsoft.AspNetCore.Mvc;
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
public class ShipmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShipmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // POST: api/shipments
    [HttpPost]
    public async Task<ActionResult<Result<ShipmentDto>>> Create([FromBody] CreateShipmentCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }

    // GET: api/shipments
    [HttpGet]
    public async Task<ActionResult<Result<IEnumerable<ShipmentDto>>>> GetAll()
    {
        var result = await _mediator.Send(new GetAllShipmentsQuery());
        return result;
    }

    // GET: api/shipments/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<ShipmentDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetShipmentByIdQuery(id));
        return result;
    }

    // POST: api/shipments/{id}/packages
    [HttpPost("{id:guid}/packages")]
    public async Task<ActionResult<Result<ShipmentDto>>> AddPackage(Guid id, [FromBody] AddPackageToShipmentCommand request)
    {
        // ensure route id is used
        request.ShipmentId = id;
        var result = await _mediator.Send(request);
        return result;
    }

    // POST: api/shipments/{id}/dispatch
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
