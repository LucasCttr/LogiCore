using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Package;

public class GetPackageByIdHandler : IRequestHandler<GetPackageByIdQuery, Result<PackageDto>>
{
    private readonly IPackageRepository _repository;
    private readonly IMapper _mapper;

    public GetPackageByIdHandler(IPackageRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PackageDto>> Handle(GetPackageByIdQuery request, CancellationToken cancellationToken)
    {
        var package = await _repository.GetByIdAsync(request.Id);
        if (package is null)
        {
            return Result<PackageDto>.Failure("Package not found");
        }

        var dto = _mapper.Map<PackageDto>(package);
        return Result<PackageDto>.Success(dto);
    }
}