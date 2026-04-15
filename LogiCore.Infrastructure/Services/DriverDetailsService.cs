using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Services;
using LogiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogiCore.Infrastructure.Services;

public class DriverDetailsService : IDriverDetailsService
{
    private readonly LogiCoreDbContext _dbContext;

    public DriverDetailsService(LogiCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<DriverDetailsWithUserDto>>> GetAllDriverDetailsAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbContext.DriverDetails
                .Include(dd => dd.User)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = searchTerm.ToLower();
                query = query.Where(dd =>
                    dd.User!.FirstName.ToLower().Contains(lowerSearch) ||
                    dd.User.LastName.ToLower().Contains(lowerSearch) ||
                    dd.User.Email.ToLower().Contains(lowerSearch) ||
                    dd.LicenseNumber.ToLower().Contains(lowerSearch)
                );
            }

            // Apply active filter
            if (isActive.HasValue)
            {
                query = query.Where(dd =>
                    (!dd.User!.LockoutEnd.HasValue || dd.User.LockoutEnd <= DateTimeOffset.UtcNow) == isActive.Value
                );
            }

            // Count total before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply pagination
            var paginatedDriverDetails = await query
                .OrderBy(dd => dd.User!.FirstName)
                .ThenBy(dd => dd.User!.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Map to DTOs - Try to find Driver by UserId, but use DriverDetails ID as fallback
            var driverIdsByUserId = await _dbContext.Drivers
                .Where(d => paginatedDriverDetails.Select(dd => dd.UserId).Contains(d.ApplicationUserId))
                .Select(d => new { d.ApplicationUserId, d.Id })
                .ToDictionaryAsync(d => d.ApplicationUserId, d => d.Id, cancellationToken);

            var driverDtos = paginatedDriverDetails.Select(dd => 
            {
                var driverId = driverIdsByUserId.ContainsKey(dd.UserId) 
                    ? driverIdsByUserId[dd.UserId] 
                    : (Guid?)null;
                
                return new DriverDetailsWithUserDto
                {
                    Id = dd.Id,
                    UserId = dd.UserId,
                    DriverId = driverId,
                    FirstName = dd.User?.FirstName ?? string.Empty,
                    LastName = dd.User?.LastName ?? string.Empty,
                    Email = dd.User?.Email ?? string.Empty,
                    IsUserActive = dd.User != null && (!dd.User.LockoutEnd.HasValue || dd.User.LockoutEnd <= DateTimeOffset.UtcNow),
                    LicenseNumber = dd.LicenseNumber,
                    LicenseType = dd.LicenseType,
                    LicenseExpiry = dd.LicenseExpiry,
                    InsuranceExpiry = dd.InsuranceExpiry,
                    AssignedVehicleId = dd.AssignedVehicleId,
                    CreatedAt = dd.CreatedAt,
                    UpdatedAt = dd.UpdatedAt
                };
            }).ToList();

            var result = new PagedResult<DriverDetailsWithUserDto>(
                Items: driverDtos,
                Total: totalCount,
                Page: pageNumber,
                PageSize: pageSize
            );

            return Result<PagedResult<DriverDetailsWithUserDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<DriverDetailsWithUserDto>>.Failure($"Error fetching driver details: {ex.Message}");
        }
    }
}
