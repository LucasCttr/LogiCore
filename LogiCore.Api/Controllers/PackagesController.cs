using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using Microsoft.AspNetCore.Mvc;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Models;
using LogiCore.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore.Metadata;
using AutoMapper;
using LogiCore.Application.Features.Package;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;


    public PackagesController(IMediator mediator, IMapper mapper, IPackageRepository repository)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllPackagesQuery(page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPackageByIdQuery(id));

        // Mapea el resultado de la capa de aplicación a una respuesta HTTP
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePackageRequest request)
    {
        var cmd = _mapper.Map<CreatePackageCommand>(request);
        var result = await _mediator.Send(cmd);
        if (!result.IsSuccess) return Ok(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, null);
    }
}