using System;
using System.Threading.Tasks;

namespace LogiCore.Application.Common.Interfaces;

public interface INotificationService
{
    Task NotifyShipmentDispatchedAsync(Guid shipmentId, string routeCode);
}
