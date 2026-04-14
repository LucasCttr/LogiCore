using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using LogiCore.Application.Features.Auth;
using LogiCore.Application.Features.User.GetAll;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<UserDto>>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 15)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 15;
        if (pageSize > 100) pageSize = 100;

        var result = await _mediator.Send(new GetAllUsersQuery(page, pageSize));
        return result;
    }

    // POST: api/users (Register a new user)
    [HttpPost]
    public async Task<ActionResult<Result<UserDto>>> Register([FromBody] RegisterUserCommand request)
    {
        var result = await _mediator.Send(request);
        return result;
    }
}
