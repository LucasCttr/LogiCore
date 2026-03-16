using LogiCore.Application.UseCases;
using LogiCore.Application.Common.Interfaces.Persistence;
using Microsoft.AspNetCore.Mvc;
using LogiCore.Api.Models.DTOs;
using LogiCore.Application.Commands;
using Microsoft.EntityFrameworkCore.Metadata;
using AutoMapper;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly CreatePackageUseCase _createPackageUseCase;
    private readonly IMapper _mapper;
    private readonly IPackageRepository _repository;

    public PackagesController(CreatePackageUseCase createPackageUseCase, IMapper mapper, IPackageRepository repository)
    {
        _createPackageUseCase = createPackageUseCase;
        _mapper = mapper;
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        throw new NotImplementedException();

    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var package = await _repository.GetByIdAsync(id);
        if (package is null) return NotFound();
        return Ok(package);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePackageRequest request)
    {
        var cmd = _mapper.Map<CreatePackageCommand>(request);
        var id = await _createPackageUseCase.ExecuteAsync(cmd);
        return CreatedAtAction(nameof(GetById), new { id }, null);
    }
}