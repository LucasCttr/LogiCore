using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Interfaces.Security;
using AutoMapper;

namespace LogiCore.Application.Features.Package.GetPackageLocation;

public class GetPackageLocationHandler : IRequestHandler<GetPackageLocationQuery, Result<ShipmentDto?>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IShipmentRepository _shipmentRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetPackageLocationHandler(IPackageRepository packageRepository, IShipmentRepository shipmentRepository, ICurrentUserService currentUserService, IMapper mapper)
    {
        _packageRepository = packageRepository;
        _shipmentRepository = shipmentRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<ShipmentDto?>> Handle(GetPackageLocationQuery request, CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(request.PackageId);
        if (package == null)
            return Result<ShipmentDto?>.Failure("Package not found.");

        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<ShipmentDto?>.Failure("Unauthorized");

        var shipment = await _shipmentRepository.GetByPackageIdAsync(request.PackageId);
        if (shipment == null)
            return Result<ShipmentDto?>.Success(null);

        return Result<ShipmentDto?>.Success(_mapper.Map<ShipmentDto>(shipment));
    }
}
