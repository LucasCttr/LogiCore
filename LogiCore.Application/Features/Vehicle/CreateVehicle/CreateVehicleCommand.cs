using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Vehicle.CreateVehicle;

public record CreateVehicleCommand(string Plate, string? Make, string? Model, decimal MaxWeightCapacity, decimal MaxVolumeCapacity) : IRequest<Result<VehicleDto>>;
