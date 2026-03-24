using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.GetAll;

public class GetAllShipmentsHandler : IRequestHandler<GetAllShipmentsQuery, Result<IEnumerable<ShipmentDto>>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IMapper _mapper;

    public GetAllShipmentsHandler(IShipmentRepository shipmentRepository, IMapper mapper)
    {
        _shipmentRepository = shipmentRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<ShipmentDto>>> Handle(GetAllShipmentsQuery request, CancellationToken cancellationToken)
    {
        var items = await _shipmentRepository.GetAllAsync();
        var dtos = items.Select(s => _mapper.Map<ShipmentDto>(s));
        return Result<IEnumerable<ShipmentDto>>.Success(dtos);
    }
}
