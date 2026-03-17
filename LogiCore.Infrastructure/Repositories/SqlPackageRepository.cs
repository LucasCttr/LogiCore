using Microsoft.EntityFrameworkCore;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;
using LogiCore.Infrastructure.Persistence;

namespace LogiCore.Infrastructure.Repositories;

public class SqlPackageRepository : IPackageRepository
{
    private readonly LogiCoreDbContext _context;

    public SqlPackageRepository(LogiCoreDbContext context) => _context = context;

    public async Task AddAsync(Package package) => await _context.Packages.AddAsync(package);

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

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}