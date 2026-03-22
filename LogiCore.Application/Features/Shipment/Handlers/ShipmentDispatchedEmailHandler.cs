using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Application.Common.Interfaces;
using LogiCore.Domain.Common.Events;

namespace LogiCore.Application.Features.Shipment.Handlers;

public class ShipmentDispatchedEmailHandler : INotificationHandler<ShipmentDispatchedEvent>
{
    private readonly IEmailService _emailService;

    public ShipmentDispatchedEmailHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(ShipmentDispatchedEvent notification, CancellationToken cancellationToken)
    {
        // This is a demo delegate.
        await _emailService.SendShipmentDispatchedEmailAsync(notification.ShipmentId, notification.RouteCode);
    }
}
