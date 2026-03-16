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

    public async Task<Package?> GetByIdAsync(Guid id) =>
        await _context.Packages.FindAsync(id);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}