using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Services;

namespace LogiCore.Application.Features.Driver.GetAll;

public class GetAllDriverDetailsQueryHandler : IRequestHandler<GetAllDriverDetailsQuery, Result<PagedResult<DriverDetailsWithUserDto>>>
{
    private readonly IDriverDetailsService _driverDetailsService;

    public GetAllDriverDetailsQueryHandler(IDriverDetailsService driverDetailsService)
    {
        _driverDetailsService = driverDetailsService;
    }

    public async Task<Result<PagedResult<DriverDetailsWithUserDto>>> Handle(GetAllDriverDetailsQuery request, CancellationToken cancellationToken)
    {
        return await _driverDetailsService.GetAllDriverDetailsAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.IsActive,
            cancellationToken);
    }
}
