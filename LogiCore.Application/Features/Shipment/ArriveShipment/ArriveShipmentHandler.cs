using System.Threading;
using System.Threading.Tasks;
using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;

namespace LogiCore.Application.Features.Shipment.ArriveShipment;

public class ArriveShipmentHandler : IRequestHandler<ArriveShipmentCommand, Result<bool>>
{
    private readonly IShipmentRepository _shipmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LogiCore.Application.Common.Interfaces.Persistence.IPackageRepository _packageRepository;

    public ArriveShipmentHandler(IShipmentRepository shipmentRepository, IUnitOfWork unitOfWork, LogiCore.Application.Common.Interfaces.Persistence.IPackageRepository packageRepository)
    {
        _shipmentRepository = shipmentRepository;
        _unitOfWork = unitOfWork;
        _packageRepository = packageRepository;
    }

    public async Task<Result<bool>> Handle(ArriveShipmentCommand request, CancellationToken cancellationToken)
    {
        var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
        if (shipment == null) return Result<bool>.Failure("Shipment not found.", ErrorType.NotFound);

        try
        {
            shipment.MarkAsArrived();

            // Move packages in the shipment to depot (batch)
            var packagesToMove = shipment.Packages
                .Where(p => p.Status == LogiCore.Domain.Entities.PackageStatus.InTransit)
                .ToList();

            foreach (var pkg in packagesToMove)
            {
                pkg.MoveToDepot();
            }

            await _shipmentRepository.UpdateAsync(shipment);
            if (packagesToMove.Any())
            {
                await _packageRepository.UpdateRangeAsync(packagesToMove);
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (LogiCore.Domain.Common.Exceptions.DomainException ex)
        {
            return Result<bool>.Failure(ex.Message);
        }
    }
}
