using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Package.GetPackageLocation;
using LogiCore.Application.Common.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using AutoMapper;
using LogiCore.Application.Features.Packages;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PackagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/packages?page=1&pageSize=20
    [HttpGet]
    public async Task<ActionResult<Result<PagedResponse<PackageDto>>>> GetByPage([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllPackagesQuery(page, pageSize));
        return result;
    }

    // GET: api/packages/{id}
    [HttpGet("{id:guid}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<PackageDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPackageByIdQuery(id));
        return result;
    }

    // POST: api/packages
    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<PackageDto>>> Create([FromBody] CreatePackageCommand request)
    {
        var result = await _mediator.Send(request);
        return result; // filter applyed in ResultActionFilter
    }

    // PUT: api/packages/{id}
    [HttpPut("{id:guid}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<Result<PackageDto>>> Update(Guid id, [FromBody] UpdatePackageCommand request)
    {
        request = request with { Id = id }; // set the id from route to the command
        var result = await _mediator.Send(request);
        return result; // filter applyed in ResultActionFilter  
    }


    // POST: api/packages/{id}/deliver
    [HttpPost("{id:guid}/deliver")]
    [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<Result<PackageDto>>> Deliver(Guid id)
    {
        var result = await _mediator.Send(new DeliverPackageCommand(id));
        return result;
    }

    // POST: api/packages/{id}/cancel
    [HttpPost("{id:guid}/cancel")]
    [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<ActionResult<Result<PackageDto>>> Cancel(Guid id)
    {
        var result = await _mediator.Send(new CancelPackageCommand(id));
        return result;
    }



    // GET: api/packages/tracking/{trackingNumber} (public minimal info)
    [HttpGet("tracking/{trackingNumber}")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<ActionResult<Result<PackagePublicLocationDto?>>> GetByTracking(string trackingNumber)
    {
        var result = await _mediator.Send(new GetPackageLocationByTrackingQuery(trackingNumber));
        return result;
    }
}