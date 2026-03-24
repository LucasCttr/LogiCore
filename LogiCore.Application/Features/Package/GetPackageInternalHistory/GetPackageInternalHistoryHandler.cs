using AutoMapper;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Application.Features.Package.GetPackageHistory;

public class GetPackageInternalHistoryHandler : IRequestHandler<GetPackageHistoryQuery, Result<IEnumerable<PackageInternalHistoryDto>>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IMapper _mapper;

    public GetPackageInternalHistoryHandler(IPackageRepository packageRepository, IMapper mapper)
    {
        _packageRepository = packageRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<PackageInternalHistoryDto>>> Handle(GetPackageHistoryQuery request, CancellationToken cancellationToken)
    {
        var histories = await _packageRepository.GetHistoryAsync(request.PackageId);
        var dtos = histories.Select(h => new PackageInternalHistoryDto(h.FromStatus.ToString(), h.ToStatus.ToString(), h.OccurredAt, h.EmployeeId, h.InternalNotes));
        return Result<IEnumerable<PackageInternalHistoryDto>>.Success(dtos);
    }
}
