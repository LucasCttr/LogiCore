using System;
using System.Threading;
using System.Threading.Tasks;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Common.Interfaces.Persistence
{
    public interface IPackageStatusHistoryService
    {
        /// <summary>
        /// Records a delivery/collection attempt failure without changing package status.
        /// </summary>
        Task AddAttemptFailedHistoryAsync(
            Guid packageId, 
            string? reason, 
            string? userId = null,
            int? locationId = null,
            Guid? shipmentId = null,
            CancellationToken cancellationToken = default);
    }
}
