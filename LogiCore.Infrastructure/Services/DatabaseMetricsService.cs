using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogiCore.Application.Common.Interfaces;
using LogiCore.Application.DTOs;
using LogiCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogiCore.Infrastructure.Services
{
    public class DatabaseMetricsService : IMetricsService
    {
        private readonly LogiCoreDbContext _db;

        public DatabaseMetricsService(LogiCoreDbContext db)
        {
            _db = db;
        }

        public async Task<Dictionary<string, object>> GetOverviewAsync()
        {
            var totalShipments = await _db.Shipments.CountAsync();
            var totalPackages = await _db.Packages.CountAsync();
            var delivered = await _db.Packages.CountAsync(p => p.Status == Domain.Entities.PackageStatus.Delivered);
            var inTransit = await _db.Packages.CountAsync(p => p.Status == Domain.Entities.PackageStatus.InTransit);
            var canceled = await _db.Packages.CountAsync(p => p.Status == Domain.Entities.PackageStatus.Canceled);

            return new Dictionary<string, object>
            {
                ["totalShipments"] = totalShipments,
                ["totalPackages"] = totalPackages,
                ["delivered"] = delivered,
                ["inTransit"] = inTransit,
                ["canceled"] = canceled
            };
        }

        public async Task<IEnumerable<DailyCountDto>> GetShipmentsPerDayAsync(int days)
        {
            // Shipments entity does not track creation timestamp; use Packages created per day as a proxy
            var from = DateTime.UtcNow.Date.AddDays(-days + 1);

            var q = await _db.Packages
                .Where(p => p.CreatedAt >= from)
                .GroupBy(p => p.CreatedAt.Date)
                .Select(g => new DailyCountDto(g.Key, g.Count()))
                .ToListAsync();

            // Ensure all days present
            var results = Enumerable.Range(0, days)
                .Select(i => from.AddDays(i))
                .Select(d => q.FirstOrDefault(x => x.Date == d) ?? new DailyCountDto(d, 0));

            return results;
        }

        public async Task<TimeSpan?> GetAverageDispatchToDeliveryAsync()
        {
            // Calculate average time between Dispatch (Shipment.Status==Dispatched event) and Delivery of packages
            // We approximate using PackageStatusHistory: from InTransit -> Delivered times per package
            var histories = await _db.PackageStatusHistories
                .Where(h => h.ToStatus == Domain.Entities.PackageStatus.Delivered)
                .ToListAsync();

            var groups = histories.GroupBy(h => h.PackageId);

            var spans = new List<TimeSpan>();
            foreach (var g in groups)
            {
                var delivered = g.FirstOrDefault(h => h.ToStatus == Domain.Entities.PackageStatus.Delivered);
                var inTransit = g.OrderBy(h => h.OccurredAt).FirstOrDefault(h => h.ToStatus == Domain.Entities.PackageStatus.InTransit || h.FromStatus == Domain.Entities.PackageStatus.InTransit);
                if (delivered != null && inTransit != null)
                {
                    spans.Add(delivered.OccurredAt - inTransit.OccurredAt);
                }
            }

            if (!spans.Any()) return null;

            var avgTicks = Convert.ToInt64(spans.Average(s => s.Ticks));
            return TimeSpan.FromTicks(avgTicks);
        }
    }
}
