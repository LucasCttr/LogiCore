using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Driver.GetAll;

public class GetAllDriverDetailsQuery : IRequest<Result<PagedResult<DriverDetailsWithUserDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 15;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
}
