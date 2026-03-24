using System;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Vehicle.GetById;

public record GetVehicleByIdQuery(Guid Id) : IRequest<Result<VehicleDto>>;
