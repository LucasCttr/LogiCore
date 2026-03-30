using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.GetPaged;

public class GetShipmentsHandler : IRequestHandler<GetShipmentsQuery, Result<PagedResultDto<ShipmentDto>>>
{
    private readonly IShipmentRepository _repo;
    private readonly IMapper _mapper;

    public GetShipmentsHandler(IShipmentRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<Result<PagedResultDto<ShipmentDto>>> Handle(GetShipmentsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _repo.GetPagedAsync(request.Page, request.PageSize, request.SortBy, request.SortDir, request.Status, request.Q);
        var dtos = items.Select(s => _mapper.Map<ShipmentDto>(s));
        var result = new PagedResultDto<ShipmentDto>(dtos, total);
        return Result<PagedResultDto<ShipmentDto>>.Success(result);
    }
}
