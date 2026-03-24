using System;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.GetById;

public record GetShipmentByIdQuery(Guid Id) : IRequest<Result<ShipmentDto>>;
