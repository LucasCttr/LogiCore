using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Models;
using LogiCore.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore.Metadata;
using AutoMapper;
using LogiCore.Application.Features.Packages;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;


    public PackagesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<PackageDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
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
    public async Task<ActionResult<Result<Guid>>> Create([FromBody] CreatePackageRequest request)
    {
        var cmd = _mapper.Map<CreatePackageCommand>(request);
        var result = await _mediator.Send(cmd);
        return result; // filter applyed in ResultActionFilter
    }
}