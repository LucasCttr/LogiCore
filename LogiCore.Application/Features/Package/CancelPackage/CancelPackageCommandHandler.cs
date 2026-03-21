

using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;
using MediatR;

namespace LogiCore.Application.Features.Packages;
public class CancelPackageCommandHandler : IRequestHandler<CancelPackageCommand, Result<PackageDto>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IMapper _mapper;
    public CancelPackageCommandHandler(IPackageRepository packageRepository, IMapper mapper)
    {
        _packageRepository = packageRepository;
        _mapper = mapper;
    }   

    public async Task<Result<PackageDto>> Handle(CancelPackageCommand request, CancellationToken cancellationToken)
    {
        var package = await _packageRepository.GetByIdAsync(request.PackageId);
        if (package == null)
        {
            return Result<PackageDto>.Failure("Package not found.");
        }

        if (package.Status == PackageStatus.Delivered)
        {
            return Result<PackageDto>.Failure("Delivered packages cannot be cancelled.");
        }

        package.Cancel();
        await _packageRepository.UpdateAsync(package);

        var packageDto = _mapper.Map<PackageDto>(package);
        return Result<PackageDto>.Success(packageDto);
    }
}