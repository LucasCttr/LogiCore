using System.Collections.Generic;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Vehicle.GetAllVehicles;

public record GetAllVehiclesQuery(): IRequest<Result<IEnumerable<VehicleDto>>>;
