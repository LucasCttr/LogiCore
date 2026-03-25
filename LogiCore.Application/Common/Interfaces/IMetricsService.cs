using System.Threading.Tasks;
using LogiCore.Application.DTOs;
using System;
using System.Collections.Generic;

namespace LogiCore.Application.Common.Interfaces
{
    public interface IMetricsService
    {
        Task<Dictionary<string, object>> GetOverviewAsync();
        Task<IEnumerable<DailyCountDto>> GetShipmentsPerDayAsync(int days);
        Task<TimeSpan?> GetAverageDispatchToDeliveryAsync();
    }
}
