using System;
using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Vehicle.DeleteVehicle;

public record DeleteVehicleCommand(Guid Id) : IRequest<Result<bool>>;
