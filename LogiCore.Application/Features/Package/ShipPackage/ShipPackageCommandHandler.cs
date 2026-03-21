using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Features.Packages;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public class ShipPackageCommandHandler : IRequestHandler<ShipPackageCommand, Result<PackageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPackageRepository _packageRepository;
    private readonly IMapper _mapper;

    public ShipPackageCommandHandler(IUnitOfWork unitOfWork, IPackageRepository packageRepository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _packageRepository = packageRepository;
        _mapper = mapper;
    }

    public async Task<Result<PackageDto>> Handle(ShipPackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(request.PackageId);
        if (package == null)
            return Result<PackageDto>.Failure("Package not found.");

        try
        {
            package.StartTransit();
        }
        catch (LogiCore.Domain.Common.Exceptions.DomainException ex)
        {
            return Result<PackageDto>.Failure(ex.Message);
        }

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<PackageDto>.Success(_mapper.Map<PackageDto>(package));
    }
}