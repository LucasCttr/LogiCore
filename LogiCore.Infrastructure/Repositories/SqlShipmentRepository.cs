using Microsoft.EntityFrameworkCore;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;
using LogiCore.Infrastructure.Persistence;

namespace LogiCore.Infrastructure.Repositories;

public class SqlShipmentRepository : IShipmentRepository
{
    private readonly LogiCoreDbContext _context;

    public SqlShipmentRepository(LogiCoreDbContext context) => _context = context;

    public async Task<Shipment> AddAsync(Shipment shipment)
    {
        var entry = await _context.Shipments.AddAsync(shipment);
        return entry.Entity;
    }

    public async Task<IEnumerable<Shipment>> GetAllAsync()
    {
        return await _context.Shipments
            .AsNoTracking()
            .Include(s => s.Packages)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Shipment> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var total = await _context.Shipments.CountAsync();
        var items = await _context.Shipments
            .AsNoTracking()
            .Include(s => s.Packages)
            .OrderBy(s => s.RouteCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Shipment?> GetByIdAsync(Guid id) =>
        await _context.Shipments
            .Include(s => s.Packages)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Shipment?> GetByPackageIdAsync(Guid packageId) =>
        await _context.Shipments
            .Include(s => s.Packages)
            .FirstOrDefaultAsync(s => s.Packages.Any(p => p.Id == packageId));

    public Task<Shipment> UpdateAsync(Shipment shipment)
    {
        var entry = _context.Shipments.Update(shipment);
        return Task.FromResult(entry.Entity);
    }
}
