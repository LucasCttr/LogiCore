using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Common.Interfaces.Persistence
{
    public interface IPackageRepository
    {
        Task<Package?> GetByIdAsync(Guid id);
        Task<IEnumerable<Package>> GetAllAsync();
        Task AddAsync(Package package);
        Task SaveChangesAsync();
    }
}