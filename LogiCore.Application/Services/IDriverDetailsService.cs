using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Services;

public interface IDriverDetailsService
{
    Task<Result<PagedResult<DriverDetailsWithUserDto>>> GetAllDriverDetailsAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}
