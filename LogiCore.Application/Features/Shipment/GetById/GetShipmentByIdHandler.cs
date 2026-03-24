using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.GetById;

public class GetShipmentByIdHandler : IRequestHandler<GetShipmentByIdQuery, Result<ShipmentDto>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IMapper _mapper;

    public GetShipmentByIdHandler(IShipmentRepository shipmentRepository, IMapper mapper)
    {
        _shipmentRepository = shipmentRepository;
        _mapper = mapper;
    }

    public async Task<Result<ShipmentDto>> Handle(GetShipmentByIdQuery request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.Id);
        if (shipment is null) return Result<ShipmentDto>.Failure("Shipment not found.");
        return Result<ShipmentDto>.Success(_mapper.Map<ShipmentDto>(shipment));
    }
}
