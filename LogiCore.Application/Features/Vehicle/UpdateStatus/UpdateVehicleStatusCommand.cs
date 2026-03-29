using MediatR;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Vehicle.UpdateStatus;

public record UpdateVehicleStatusCommand(System.Guid Id, string Status) : IRequest<Result<VehicleDto>>;
