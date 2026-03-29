using Microsoft.EntityFrameworkCore;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;
using LogiCore.Infrastructure.Persistence;

namespace LogiCore.Infrastructure.Repositories;

public class SqlLocationRepository : ILocationRepository
{
    private readonly LogiCoreDbContext _context;
    public SqlLocationRepository(LogiCoreDbContext context) => _context = context;

    public async Task<Location?> GetByIdAsync(Guid id) =>
        await _context.Set<Location>().FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IEnumerable<Location>> GetAllAsync() =>
        await _context.Set<Location>().AsNoTracking().ToListAsync();

    public async Task<Location> AddAsync(Location location)
    {
        var entry = await _context.Set<Location>().AddAsync(location);
        return entry.Entity;
    }

    public Task<Location> UpdateAsync(Location location)
    {
        var entry = _context.Set<Location>().Update(location);
        return Task.FromResult(entry.Entity);
    }
}
