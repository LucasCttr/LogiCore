
using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;
using MediatR;

namespace LogiCore.Application.Features.Packages;

public class DeliverPackageCommandHandler : IRequestHandler<DeliverPackageCommand, Result<PackageDto>>  
{
    private readonly IPackageRepository _packageRepository;
    private readonly IMapper _mapper;

    public DeliverPackageCommandHandler(IPackageRepository packageRepository, IMapper mapper)
    {
        _packageRepository = packageRepository;
        _mapper = mapper;
    }

    public async Task<Result<PackageDto>> Handle(DeliverPackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(request.PackageId);
        if (package == null)
        {
            return Result<PackageDto>.Failure("Package not found.");
        }

        if (package.Status != PackageStatus.InTransit)
        {
            return Result<PackageDto>.Failure("Only packages in transit can be delivered.");
        }

        package.Deliver();
        await _packageRepository.UpdateAsync(package);

        var packageDto = _mapper.Map<PackageDto>(package);
        return Result<PackageDto>.Success(packageDto);
    }
}