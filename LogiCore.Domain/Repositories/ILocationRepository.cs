using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Common.Interfaces.Persistence
{
    public interface ILocationRepository
    {
        Task<Location?> GetByIdAsync(Guid id);
        Task<IEnumerable<Location>> GetAllAsync();
        Task<Location> AddAsync(Location location);
        Task<Location> UpdateAsync(Location location);
    }
}
