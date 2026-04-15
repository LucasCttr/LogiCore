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
        var historiesWithUsers = await _packageRepository.GetHistoryWithUserAsync(request.PackageId);
        
        var dtos = historiesWithUsers.Select(item => 
        {
            var history = item.Item1;
            var user = item.Item2;
            var roles = item.Item3;
            var rolesString = roles.Any() ? string.Join(", ", roles) : null;
            
            return new PackageInternalHistoryDto(
                history.FromStatus == (LogiCore.Domain.Entities.PackageStatus)(-1) ? "Created" : history.FromStatus.ToString(), 
                history.ToStatus.ToString(), 
                history.OccurredAt,
                history.UserId,
                user?.Email,
                user?.FirstName,
                user?.LastName,
                rolesString,
                history.LocationId,
                history.ShipmentId,
                history.Notes,
                history.EmployeeId,      // Legacy
                history.InternalNotes    // Legacy
            )
            {
                Id = history.Id,
                PackageId = history.PackageId
            };
        });
        
        return Result<IEnumerable<PackageInternalHistoryDto>>.Success(dtos);
    }
}
