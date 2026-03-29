using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Driver.UpdateStatus;

public class UpdateDriverStatusCommand : IRequest<Result<LogiCore.Application.DTOs.DriverDto>>
{
    public Guid DriverId { get; set; }
    public bool IsActive { get; set; }
}
