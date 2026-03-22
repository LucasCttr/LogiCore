using System;
using System.Threading.Tasks;
using LogiCore.Application.Common.Interfaces;

namespace LogiCore.Infrastructure.Services;

public class ConsoleNotificationService : INotificationService
{
    public Task NotifyShipmentDispatchedAsync(Guid shipmentId, string routeCode)
    {
        // Simple, synchronous console notification for demo/testing purposes
        Console.WriteLine($"[Notification] Shipment dispatched: {shipmentId} (Route: {routeCode})");
        return Task.CompletedTask;
    }
}
