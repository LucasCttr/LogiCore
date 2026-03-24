using System;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Vehicle.UpdateVehicle;

public record UpdateVehicleCommand(Guid Id, string Plate, decimal MaxWeightCapacity, decimal MaxVolumeCapacity, bool IsActive) : IRequest<Result<VehicleDto>>;
