using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Common.Interfaces.Persistence
{
    public interface IDriverRepository
    {
        Task<Driver?> GetByIdAsync(Guid id);
        Task<Driver?> GetByApplicationUserIdAsync(string applicationUserId);
        Task<IEnumerable<Driver>> GetAllAsync();
        Task<IEnumerable<Driver>> GetAvailableAsync();
        Task<Driver> AddAsync(Driver driver);
        Task<Driver> UpdateAsync(Driver driver);
    }
}
