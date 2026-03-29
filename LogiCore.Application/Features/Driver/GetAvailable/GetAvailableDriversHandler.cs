using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Driver.GetAvailable;

public class GetAvailableDriversHandler : IRequestHandler<GetAvailableDriversQuery, Result<IEnumerable<DriverDto>>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IMapper _mapper;

    public GetAvailableDriversHandler(IDriverRepository driverRepository, IMapper mapper)
    {
        _driverRepository = driverRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<DriverDto>>> Handle(GetAvailableDriversQuery request, CancellationToken cancellationToken)
    {
        var drivers = await _driverRepository.GetAvailableAsync();
        var dtos = drivers.Select(d => _mapper.Map<DriverDto>(d));
        return Result<IEnumerable<DriverDto>>.Success(dtos);
    }
}
