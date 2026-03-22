using System.Threading.Tasks;
using System;

namespace LogiCore.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendShipmentDispatchedEmailAsync(Guid shipmentId, string routeCode);
}
