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
            var allDriverDetails = await _dbContext.DriverDetails
                .AsNoTracking()
                .Include(dd => dd.User)
                .ToListAsync(cancellationToken);

            var filteredDriverDetails = allDriverDetails
                .Where(dd =>
                {
                    var isCurrentlyActive = dd.User != null && (!dd.User.LockoutEnd.HasValue || dd.User.LockoutEnd <= DateTimeOffset.UtcNow);

                    if (isActive.HasValue && isCurrentlyActive != isActive.Value)
                    {
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(searchTerm))
                    {
                        return true;
                    }

                    var lowerSearch = searchTerm.Trim().ToLowerInvariant();
                    var firstName = dd.User?.FirstName?.ToLowerInvariant() ?? string.Empty;
                    var lastName = dd.User?.LastName?.ToLowerInvariant() ?? string.Empty;
                    var email = dd.User?.Email?.ToLowerInvariant() ?? string.Empty;
                    var licenseNumber = dd.LicenseNumber?.ToLowerInvariant() ?? string.Empty;

                    return firstName.Contains(lowerSearch)
                        || lastName.Contains(lowerSearch)
                        || email.Contains(lowerSearch)
                        || licenseNumber.Contains(lowerSearch);
                })
                .OrderBy(dd => dd.User?.FirstName ?? string.Empty)
                .ThenBy(dd => dd.User?.LastName ?? string.Empty)
                .ToList();

            var totalCount = filteredDriverDetails.Count;

            var paginatedDriverDetails = filteredDriverDetails
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var vehicleIds = paginatedDriverDetails
                .Where(dd => dd.AssignedVehicleId.HasValue)
                .Select(dd => dd.AssignedVehicleId!.Value)
                .Distinct()
                .ToList();

            var vehiclesById = await _dbContext.Set<LogiCore.Domain.Entities.Vehicle>()
                .Where(v => vehicleIds.Contains(v.Id))
                .Select(v => new { v.Id, v.Plate, v.Make, v.Model })
                .ToDictionaryAsync(v => v.Id, v => new { v.Plate, v.Make, v.Model }, cancellationToken);

            // Map to DTOs - enrich DriverDetails with Driver aggregate data when placeholders exist.
            var driversByUserId = await _dbContext.Drivers
                .Where(d => paginatedDriverDetails.Select(dd => dd.UserId).Contains(d.ApplicationUserId))
                .Select(d => new { d.ApplicationUserId, d.Id, d.LicenseNumber })
                .ToDictionaryAsync(d => d.ApplicationUserId, d => new { d.Id, d.LicenseNumber }, cancellationToken);

            var driverDtos = paginatedDriverDetails.Select(dd => 
            {
                var hasDriver = driversByUserId.TryGetValue(dd.UserId, out var driverData);
                var driverId = hasDriver ? driverData!.Id : (Guid?)null;
                var resolvedLicenseNumber = IsPlaceholder(dd.LicenseNumber) && hasDriver && !string.IsNullOrWhiteSpace(driverData!.LicenseNumber)
                    ? driverData.LicenseNumber
                    : dd.LicenseNumber;
                var resolvedLicenseType = ResolveLicenseType(dd.LicenseType, resolvedLicenseNumber);
                string? assignedVehiclePlate = null;
                string? assignedVehicleMake = null;
                string? assignedVehicleModel = null;
                if (dd.AssignedVehicleId.HasValue)
                {
                    if (vehiclesById.TryGetValue(dd.AssignedVehicleId.Value, out var foundVehicleData))
                    {
                        assignedVehiclePlate = foundVehicleData.Plate;
                        assignedVehicleMake = foundVehicleData.Make;
                        assignedVehicleModel = foundVehicleData.Model;
                    }
                }
                
                return new DriverDetailsWithUserDto
                {
                    Id = dd.Id,
                    UserId = dd.UserId,
                    DriverId = driverId,
                    FirstName = dd.User?.FirstName ?? string.Empty,
                    LastName = dd.User?.LastName ?? string.Empty,
                    Email = dd.User?.Email ?? string.Empty,
                    IsUserActive = dd.User != null && (!dd.User.LockoutEnd.HasValue || dd.User.LockoutEnd <= DateTimeOffset.UtcNow),
                    LicenseNumber = resolvedLicenseNumber,
                    LicenseType = resolvedLicenseType,
                    LicenseExpiry = dd.LicenseExpiry,
                    InsuranceExpiry = dd.InsuranceExpiry,
                    AssignedVehicleId = dd.AssignedVehicleId,
                    AssignedVehiclePlate = assignedVehiclePlate,
                    AssignedVehicleMake = assignedVehicleMake,
                    AssignedVehicleModel = assignedVehicleModel,
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

    private static bool IsPlaceholder(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            || string.Equals(value.Trim(), "TBD", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveLicenseType(string? currentLicenseType, string? resolvedLicenseNumber)
    {
        if (!IsPlaceholder(currentLicenseType))
        {
            return currentLicenseType!;
        }

        if (!string.IsNullOrWhiteSpace(resolvedLicenseNumber))
        {
            var parts = resolvedLicenseNumber.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[0]))
            {
                return parts[0];
            }
        }

        return "Not specified";
    }
}
