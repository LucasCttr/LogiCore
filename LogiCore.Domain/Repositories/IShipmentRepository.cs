using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Common.Interfaces.Persistence
{
    public interface IShipmentRepository
    {
        Task<Shipment?> GetByIdAsync(Guid id);
        Task<Shipment?> GetByPackageIdAsync(Guid packageId);
        Task<IEnumerable<Shipment>> GetAllAsync();
        Task<IEnumerable<Shipment>> GetByDriverIdAsync(Guid driverId);
        Task<(IEnumerable<Shipment> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
        Task<Shipment> AddAsync(Shipment shipment);
        Task<Shipment> UpdateAsync(Shipment shipment);
    }
}
