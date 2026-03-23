using Microsoft.EntityFrameworkCore;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;
using LogiCore.Infrastructure.Persistence;

namespace LogiCore.Infrastructure.Repositories;

public class SqlVehicleRepository : IVehicleRepository
{
    private readonly LogiCoreDbContext _context;

    public SqlVehicleRepository(LogiCoreDbContext context) => _context = context;

    public async Task<Vehicle?> GetByIdAsync(Guid id) =>
        await _context.Set<Vehicle>().Include(v => v.Shipments).FirstOrDefaultAsync(v => v.Id == id);

    public async Task<IEnumerable<Vehicle>> GetAllAsync() =>
        await _context.Set<Vehicle>().AsNoTracking().ToListAsync();

    public async Task<Vehicle> AddAsync(Vehicle vehicle)
    {
        var entry = await _context.Set<Vehicle>().AddAsync(vehicle);
        return entry.Entity;
    }

    public Task<Vehicle> UpdateAsync(Vehicle vehicle)
    {
        var entry = _context.Set<Vehicle>().Update(vehicle);
        return Task.FromResult(entry.Entity);
    }
}
