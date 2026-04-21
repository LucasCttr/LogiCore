using System;
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
        // 1. Obtenemos los envíos paginados
        var (items, total) = await _repo.GetPagedAsync(
            request.Page, 
            request.PageSize, 
            request.SortBy, 
            request.SortDir, 
            request.Status, 
            request.Q);

        // 2. Obtenemos todas las locaciones para el mapeo de nombres
        var locations = await _locationRepository.GetAllAsync();

        // 3. Creamos el diccionario usando string como llave. 
        // Esto evita errores de formato si comparamos Guid vs Int.
        var locationLookup = locations.ToDictionary(
            location => location.Id.ToString().ToLower(), 
            location => location.Name);

        // 4. Mapeamos los resultados
        var dtos = items.Select(s => MapShipment(s, locationLookup)).ToList();
        
        var result = new PagedResultDto<ShipmentDto>(dtos, total);
        return Result<PagedResultDto<ShipmentDto>>.Success(result);
    }

    private ShipmentDto MapShipment(LogiCore.Domain.Entities.Shipment shipment, IReadOnlyDictionary<string, string> locationLookup)
    {
        var dto = _mapper.Map<ShipmentDto>(shipment);
        
        // Asignamos los nombres buscando por el string del ID
        dto.OriginLocationName = GetLocationName(locationLookup, shipment.OriginLocationId);
        dto.DestinationLocationName = GetLocationName(locationLookup, shipment.DestinationLocationId);
        
        return dto;
    }

    private static string? GetLocationName(IReadOnlyDictionary<string, string> locationLookup, int? locationId)
    {
        if (!locationId.HasValue)
        {
            return null;
        }

        // Convertimos el int? a string para buscar en el diccionario sin Parsear Guids
        var searchKey = locationId.Value.ToString().ToLower();

        return locationLookup.TryGetValue(searchKey, out var name)
            ? name
            : $"Loc ID: {locationId}"; // Fallback para no devolver null si no lo encuentra
    }
}