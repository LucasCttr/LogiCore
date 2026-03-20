using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.DTOs;
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

    [HttpGet]
    public async Task<ActionResult<Result<PagedResponse<PackageDto>>>> GetByPage([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllPackagesQuery(page, pageSize));
        return result;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Result<PackageDto>>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPackageByIdQuery(id));
        return result;
    }

    [HttpPost]
    public async Task<ActionResult<Result<PackageDto>>> Create([FromBody] CreatePackageCommand request)
    {
        var result = await _mediator.Send(request);
        return result; // filter applyed in ResultActionFilter
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<PackageDto>>> Update(Guid id, [FromBody] UpdatePackageCommand request)
    {
        request = request with { Id = id }; // set the id from route to the command
        var result = await _mediator.Send(request);
        return result; // filter applyed in ResultActionFilter  
    }
}