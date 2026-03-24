using System.Collections.Generic;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.GetAll;

public record GetAllShipmentsQuery() : IRequest<Result<IEnumerable<ShipmentDto>>>;
