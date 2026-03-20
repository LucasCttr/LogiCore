using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public AuthController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Result<UserDto>>> Register([FromBody] RegisterUserCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<UserDto>>> Login([FromBody] LoginUserCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }
}
