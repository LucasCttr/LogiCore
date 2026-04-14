using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public class GetPackageByIdHandler : IRequestHandler<GetPackageByIdQuery, Result<PackageDetailDto>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IMapper _mapper;

    public GetPackageByIdHandler(IPackageRepository packageRepository, IShipmentRepository shipmentRepository, IMapper mapper)
    {
        _packageRepository = packageRepository;
        _shipmentRepository = shipmentRepository;
        _mapper = mapper;
    }

    public async Task<Result<PackageDetailDto>> Handle(GetPackageByIdQuery request, CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(request.Id);
        if (package is null)
        {
            return Result<PackageDetailDto>.Failure("Package not found");
        }

        var dto = _mapper.Map<PackageDetailDto>(package);

        // Include current shipment info if package is in transit
        if (package.CurrentShipmentId.HasValue)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(package.CurrentShipmentId.Value);
            if (shipment != null)
            {
                dto = dto with
                {
                    CurrentShipment = new CurrentShipmentDto
                    {
                        Id = shipment.Id,
                        Type = shipment.Type,
                        DestinationName = shipment.Type == LogiCore.Domain.Entities.ShipmentType.Transfer ? 
                            $"Depot (ID: {shipment.DestinationLocationId})" : null,
                        DestinationLocationId = shipment.DestinationLocationId
                    }
                };
            }
        }

        return Result<PackageDetailDto>.Success(dto);
    }
}