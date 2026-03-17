using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.DTOs;
using System.Linq;
using AutoMapper;

namespace LogiCore.Application.Features.Package;

public class GetAllPackagesHandler : IRequestHandler<GetAllPackagesQuery, PagedResponse<PackageDto>>
{
    private readonly IPackageRepository _repository;
    private readonly IMapper _mapper;

    public GetAllPackagesHandler(IPackageRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PackageDto>> Handle(GetAllPackagesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _repository.GetPagedAsync(request.Page, request.PageSize);

        var dtos = _mapper.Map<IEnumerable<PackageDto>>(items);

        return PagedResponse<PackageDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
