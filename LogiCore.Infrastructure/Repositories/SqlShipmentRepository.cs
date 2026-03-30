using System;
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
            .Include(s => s.Vehicle)
            .Include(s => s.Driver)
            .ToListAsync();
    }

    public async Task<IEnumerable<Shipment>> GetByDriverIdAsync(Guid driverId)
    {
        return await _context.Shipments
            .AsNoTracking()
            .Include(s => s.Packages)
            .Include(s => s.Vehicle)
            .Include(s => s.Driver)
            .Where(s => s.DriverId == driverId)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Shipment> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? sortBy = null, string? sortDir = null, string? status = null, string? q = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.Shipments
            .AsNoTracking()
            .Include(s => s.Packages)
            .Include(s => s.Vehicle)
            .Include(s => s.Driver)
            .AsQueryable();

        // Filtering by status (parse to enum)
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<ShipmentStatus>(status.Trim(), true, out var statusEnum))
            {
                query = query.Where(s => s.Status == statusEnum);
            }
        }

        // Simple search across route code, vehicle plate, driver name, or package tracking number
        if (!string.IsNullOrWhiteSpace(q))
        {
            var qTrim = q.Trim();
            query = query.Where(s =>
                s.RouteCode.Contains(qTrim) ||
                (s.Vehicle != null && s.Vehicle.Plate.Contains(qTrim)) ||
                (s.Driver != null && s.Driver.Name.Contains(qTrim)) ||
                s.Packages.Any(p => p.TrackingNumber.Contains(qTrim))
            );
        }

        // Total before paging
        var total = await query.CountAsync();

        // Sorting
        if (string.IsNullOrWhiteSpace(sortBy)) sortBy = "routeCode";
        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
        // Apply simple sorting switches for known fields
        query = sortBy.ToLowerInvariant() switch
        {
            "status" => desc ? query.OrderByDescending(s => s.Status) : query.OrderBy(s => s.Status),
            "estimateddelivery" => desc ? query.OrderByDescending(s => s.EstimatedDelivery) : query.OrderBy(s => s.EstimatedDelivery),
            "createdat" => desc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            "vehicleplate" => desc ? query.OrderByDescending(s => s.Vehicle!.Plate) : query.OrderBy(s => s.Vehicle!.Plate),
            _ => desc ? query.OrderByDescending(s => s.RouteCode) : query.OrderBy(s => s.RouteCode),
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (items, total);
    }

    public async Task<Shipment?> GetByIdAsync(Guid id) =>
        await _context.Shipments
            .Include(s => s.Packages)
            .Include(s => s.Vehicle)
            .Include(s => s.Driver)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Shipment?> GetByPackageIdAsync(Guid packageId) =>
        await _context.Shipments
            .Include(s => s.Packages)
            .Include(s => s.Vehicle)
            .Include(s => s.Driver)
            .FirstOrDefaultAsync(s => s.Packages.Any(p => p.Id == packageId));

    public Task<Shipment> UpdateAsync(Shipment shipment)
    {
        var entry = _context.Shipments.Update(shipment);
        return Task.FromResult(entry.Entity);
    }
}
