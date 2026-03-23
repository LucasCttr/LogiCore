using AutoMapper;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Shipment.AddPackageToShipment;

public class AddPackageToShipmentCommandHandler : IRequestHandler<AddPackageToShipmentCommand, Result<ShipmentDto>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AddPackageToShipmentCommandHandler(IShipmentRepository shipmentRepository, IPackageRepository packageRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _shipmentRepository = shipmentRepository;
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ShipmentDto>> Handle(AddPackageToShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment == null)
            return Result<ShipmentDto>.Failure("Shipment not found.");

        var package = await _packageRepository.GetByIdAsync(request.PackageId);
        if (package == null)
            return Result<ShipmentDto>.Failure("Package not found.");

        try
        {
            shipment.AddPackage(package);
        }
        catch (LogiCore.Domain.Common.Exceptions.DomainException ex)
        {
            return Result<ShipmentDto>.Failure(ex.Message);
        }

            // Mark shipment as updated and commit once to ensure transactional consistency.
            await _shipmentRepository.UpdateAsync(shipment);
            await _unitOfWork.CommitAsync(cancellationToken);

        return Result<ShipmentDto>.Success(_mapper.Map<ShipmentDto>(shipment));
    }
}
