using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Location.GetAll;

public class GetAllLocationsHandler : IRequestHandler<GetAllLocationsQuery, Result<IEnumerable<LocationDto>>>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IMapper _mapper;

    public GetAllLocationsHandler(ILocationRepository locationRepository, IMapper mapper)
    {
        _locationRepository = locationRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<LocationDto>>> Handle(GetAllLocationsQuery request, CancellationToken cancellationToken)
    {
        var items = await _locationRepository.GetAllAsync();
        var dtos = items.Select(l => _mapper.Map<LocationDto>(l));
        return Result<IEnumerable<LocationDto>>.Success(dtos);
    }
}
