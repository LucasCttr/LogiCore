using Microsoft.EntityFrameworkCore;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;
using LogiCore.Infrastructure.Persistence;

namespace LogiCore.Infrastructure.Repositories;

public class SqlPackageRepository : IPackageRepository
{
    private readonly LogiCoreDbContext _context;

    public SqlPackageRepository(LogiCoreDbContext context) => _context = context;

    public async Task<Package> AddAsync(Package package)
    {
        var entry = await _context.Packages.AddAsync(package);
        return entry.Entity;
    }

    public async Task<bool> ExistsByTrackingNumberAsync(string trackingNumber)
    {
        return await _context.Packages
            .AsNoTracking()
            .AnyAsync(p => p.TrackingNumber == trackingNumber);
    }

    public async Task<IEnumerable<Package>> GetAllAsync()
    {
        return await _context.Packages.AsNoTracking().ToListAsync();
    }

    public async Task<(IEnumerable<Package> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var total = await _context.Packages.CountAsync();
        var items = await _context.Packages
            .AsNoTracking()
            .OrderBy(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Package?> GetByIdAsync(Guid id) =>
        await _context.Packages.FindAsync(id);

    public async Task<Package?> GetByTrackingNumberAsync(string trackingNumber) =>
        await _context.Packages.AsNoTracking().FirstOrDefaultAsync(p => p.TrackingNumber == trackingNumber);

    public async Task<IEnumerable<PackageStatusHistory>> GetHistoryAsync(Guid packageId)
    {
        return await _context.PackageStatusHistories
            .AsNoTracking()
            .Where(h => h.PackageId == packageId)
            .OrderByDescending(h => h.OccurredAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<(PackageStatusHistory, LogiCore.Domain.Entities.ApplicationUser?, IList<string>)>> GetHistoryWithUserAsync(Guid packageId)
    {
        var histories = await _context.PackageStatusHistories
            .AsNoTracking()
            .Where(h => h.PackageId == packageId)
            .OrderByDescending(h => h.OccurredAt)
            .ToListAsync();

        var result = new List<(PackageStatusHistory, LogiCore.Domain.Entities.ApplicationUser?, IList<string>)>();

        foreach (var history in histories)
        {
            LogiCore.Domain.Entities.ApplicationUser? user = null;
            IList<string> roles = new List<string>();

            if (!string.IsNullOrEmpty(history.UserId))
            {
                user = await _context.Users.FindAsync(history.UserId);
                if (user != null)
                {
                    roles = await _context.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .Join(_context.Roles,
                              ur => ur.RoleId,
                              r => r.Id,
                              (ur, r) => r.Name)
                        .ToListAsync();
                }
            }

            result.Add((history, user, roles));
        }

        return result;
    }

    public Task<Package> UpdateAsync(Package package)
    {
        var entry = _context.Packages.Update(package);
        // Do not call SaveChanges here: commit is handled by UnitOfWork/SaveChangesBehavior
        return Task.FromResult(entry.Entity);
    }

    public async Task<IEnumerable<Package>> GetManyByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return Enumerable.Empty<Package>();

        return await _context.Packages
            .Where(p => idList.Contains(p.Id))
            .ToListAsync();
    }

    public Task UpdateRangeAsync(IEnumerable<Package> packages)
    {
        _context.Packages.UpdateRange(packages);
        return Task.CompletedTask;
    }
}