using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Entities;
using LogiCore.Application.DTOs;
using AutoMapper;

namespace LogiCore.Application.Features.Packages;
public class CreatePackageCommandHandler : IRequestHandler<CreatePackageCommand, Result<PackageDto>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IMapper _mapper;

    public CreatePackageCommandHandler(IPackageRepository packageRepository, IMapper mapper)
    {
        _packageRepository = packageRepository;
        _mapper = mapper;
    }

    public async Task<Result<PackageDto>> Handle(CreatePackageCommand request, CancellationToken cancellationToken)
    {
        var package = Package.Create(request.TrackingNumber, request.RecipientName, request.Weight);
        var added = await _packageRepository.AddAsync(package);
        // Do not call SaveChanges here; SaveChanges will be executed by the SaveChangesBehavior (UnitOfWork) after handler completes
            return Result<PackageDto>.Success(_mapper.Map<PackageDto>(added));
    }
}