using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Driver.GetAll;

public class GetAllDriversHandler : IRequestHandler<GetAllDriversQuery, Result<IEnumerable<DriverDto>>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IMapper _mapper;

    public GetAllDriversHandler(IDriverRepository driverRepository, IMapper mapper)
    {
        _driverRepository = driverRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<DriverDto>>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
    {
        var drivers = await _driverRepository.GetAllAsync();
        var dtos = drivers.Select(d => _mapper.Map<DriverDto>(d));
        return Result<IEnumerable<DriverDto>>.Success(dtos);
    }
}
