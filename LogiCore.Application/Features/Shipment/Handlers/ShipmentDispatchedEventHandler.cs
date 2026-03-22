using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Application.Common.Interfaces;
using LogiCore.Domain.Common.Events;

namespace LogiCore.Application.Features.Shipment.Handlers;

public class ShipmentDispatchedEventHandler : INotificationHandler<ShipmentDispatchedEvent>
{
    private readonly INotificationService _notificationService;

    public ShipmentDispatchedEventHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task Handle(ShipmentDispatchedEvent notification, CancellationToken cancellationToken)
    {
        // Example: delegate real work to an infrastructure service
        await _notificationService.NotifyShipmentDispatchedAsync(notification.ShipmentId, notification.RouteCode);
    }
}
