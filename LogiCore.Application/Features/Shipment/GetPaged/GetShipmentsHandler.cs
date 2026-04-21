using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using AutoMapper;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.GetPaged;

public class GetShipmentsHandler : IRequestHandler<GetShipmentsQuery, Result<PagedResultDto<ShipmentDto>>>
{
    private readonly IShipmentRepository _repo;
    private readonly ILocationRepository _locationRepository;
    private readonly IMapper _mapper;

    public GetShipmentsHandler(IShipmentRepository repo, ILocationRepository locationRepository, IMapper mapper)
    {
        _repo = repo;
        _locationRepository = locationRepository;
        _mapper = mapper;
    }

    public async Task<Result<PagedResultDto<ShipmentDto>>> Handle(GetShipmentsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _repo.GetPagedAsync(request.Page, request.PageSize, request.SortBy, request.SortDir, request.Status, request.Q);
        var locations = await _locationRepository.GetAllAsync();
        var locationLookup = locations.ToDictionary(location => location.Id, location => location.Name);

        var dtos = items.Select(s => MapShipment(s, locationLookup));
        var result = new PagedResultDto<ShipmentDto>(dtos, total);
        return Result<PagedResultDto<ShipmentDto>>.Success(result);
    }

    private ShipmentDto MapShipment(LogiCore.Domain.Entities.Shipment shipment, IReadOnlyDictionary<Guid, string> locationLookup)
    {
        var dto = _mapper.Map<ShipmentDto>(shipment);
        dto.OriginLocationName = GetLocationName(locationLookup, shipment.OriginLocationId);
        dto.DestinationLocationName = GetLocationName(locationLookup, shipment.DestinationLocationId);
        return dto;
    }

    private static string? GetLocationName(IReadOnlyDictionary<Guid, string> locationLookup, int? locationId)
    {
        if (!locationId.HasValue)
        {
            return null;
        }

        return locationLookup.TryGetValue(Guid.Parse(locationId.Value.ToString()), out var name)
            ? name
            : null;
    }
}
