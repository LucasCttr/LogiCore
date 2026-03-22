using System;
using System.Threading.Tasks;
using LogiCore.Application.Common.Interfaces;

namespace LogiCore.Infrastructure.Services;

/// <summary>
/// Demo SMTP-like email service. Replace with real SMTP/SendGrid implementation in production.
/// </summary>
public class SmtpEmailService : IEmailService
{
    public Task SendShipmentDispatchedEmailAsync(Guid shipmentId, string routeCode)
    {
        Console.WriteLine($"[Email] Shipment {shipmentId} dispatched for route {routeCode} (demo email sent)");
        return Task.CompletedTask;
    }
}
