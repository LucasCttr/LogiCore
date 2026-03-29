using System;
using System.Collections.Generic;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.GetByDriver;

public record GetShipmentsByDriverQuery(Guid DriverId) : IRequest<Result<IEnumerable<ShipmentDto>>>;
