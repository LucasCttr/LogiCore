using LogiCore.Application.Repositories;
using LogiCore.Domain.Entities;
using LogiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogiCore.Infrastructure.Repositories;

public class DriverDetailsRepository : IDriverDetailsRepository
{
    private readonly LogiCoreDbContext _dbContext;

    public DriverDetailsRepository(LogiCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DriverDetails?> GetByIdAsync(Guid id)
    {
        return await _dbContext.DriverDetails
            .Include(dd => dd.User)
            .Include(dd => dd.AssignedVehicle)
            .FirstOrDefaultAsync(dd => dd.Id == id);
    }

    public async Task<DriverDetails?> GetByUserIdAsync(string userId)
    {
        return await _dbContext.DriverDetails
            .Include(dd => dd.User)
            .Include(dd => dd.AssignedVehicle)
            .FirstOrDefaultAsync(dd => dd.UserId == userId);
    }

    public async Task<DriverDetails> CreateAsync(DriverDetails driverDetails)
    {
        _dbContext.DriverDetails.Add(driverDetails);
        await _dbContext.SaveChangesAsync();
        return driverDetails;
    }

    public async Task<DriverDetails> UpdateAsync(DriverDetails driverDetails)
    {
        _dbContext.DriverDetails.Update(driverDetails);
        await _dbContext.SaveChangesAsync();
        return driverDetails;
    }

    public async Task DeleteAsync(string userId)
    {
        var driverDetails = await GetByUserIdAsync(userId);
        if (driverDetails != null)
        {
            _dbContext.DriverDetails.Remove(driverDetails);
            await _dbContext.SaveChangesAsync();
        }
    }
}
