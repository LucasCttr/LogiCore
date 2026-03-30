using MediatR;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.GetPaged;

public record GetShipmentsQuery(int Page, int PageSize, string? SortBy, string? SortDir, string? Status, string? Q) : IRequest<Result<LogiCore.Application.DTOs.PagedResultDto<LogiCore.Application.DTOs.ShipmentDto>>>;
