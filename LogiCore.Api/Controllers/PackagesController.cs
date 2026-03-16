using LogiCore.Application.UseCases;
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

    public PackagesController(CreatePackageUseCase createPackageUseCase, IMapper mapper)
    {
        _createPackageUseCase = createPackageUseCase;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        throw new NotImplementedException();

    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePackageRequest request)
    {
        var cmd = _mapper.Map<CreatePackageCommand>(request);
        await _createPackageUseCase.ExecuteAsync(cmd);
        return Ok();

    }
}