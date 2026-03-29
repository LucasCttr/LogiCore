using Microsoft.EntityFrameworkCore;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;
using LogiCore.Infrastructure.Persistence;

namespace LogiCore.Infrastructure.Repositories;

public class SqlDriverRepository : IDriverRepository
{
    private readonly LogiCoreDbContext _context;

    public SqlDriverRepository(LogiCoreDbContext context) => _context = context;

    public async Task<Driver?> GetByIdAsync(Guid id) =>
        await _context.Set<Driver>().Include(d => d.Shipments).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<Driver>> GetAllAsync() =>
        await _context.Set<Driver>().AsNoTracking().ToListAsync();

    public async Task<Driver> AddAsync(Driver driver)
    {
        var entry = await _context.Set<Driver>().AddAsync(driver);
        return entry.Entity;
    }

    public Task<Driver> UpdateAsync(Driver driver)
    {
        var entry = _context.Set<Driver>().Update(driver);
        return Task.FromResult(entry.Entity);
    }
}
