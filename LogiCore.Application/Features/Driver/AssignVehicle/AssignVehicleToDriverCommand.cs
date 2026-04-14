using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using System;

namespace LogiCore.Application.Features.Driver;

public class AssignVehicleToDriverCommand : IRequest<Result<DriverDto>>
{
    public required Guid DriverId { get; init; }
    public Guid? VehicleId { get; init; }
}
