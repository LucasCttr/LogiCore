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
        Task<Package?> GetByTrackingNumberAsync(string trackingNumber);
        Task<IEnumerable<PackageStatusHistory>> GetHistoryAsync(Guid packageId);
        Task<IEnumerable<(PackageStatusHistory, LogiCore.Domain.Entities.ApplicationUser?, IList<string>)>> GetHistoryWithUserAsync(Guid packageId);
        Task<IEnumerable<Package>> GetAllAsync();
        Task<(IEnumerable<Package> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
        Task<bool> ExistsByTrackingNumberAsync(string trackingNumber);
        Task<Package> AddAsync(Package package);
        Task<Package> UpdateAsync(Package package);
        Task<IEnumerable<Package>> GetManyByIdsAsync(IEnumerable<Guid> ids);
        Task UpdateRangeAsync(IEnumerable<Package> packages);
        Task UpdateCurrentLocationBulkAsync(IEnumerable<Guid> packageIds, int locationId);
    }
}