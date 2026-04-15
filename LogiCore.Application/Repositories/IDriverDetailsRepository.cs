using LogiCore.Domain.Entities;

namespace LogiCore.Application.Repositories;

public interface IDriverDetailsRepository
{
    Task<DriverDetails?> GetByIdAsync(Guid id);
    Task<DriverDetails?> GetByUserIdAsync(string userId);
    Task<DriverDetails> CreateAsync(DriverDetails driverDetails);
    Task<DriverDetails> UpdateAsync(DriverDetails driverDetails);
    Task DeleteAsync(string userId);
}
