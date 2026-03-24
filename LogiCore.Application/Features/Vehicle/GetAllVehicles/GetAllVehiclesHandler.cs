using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Interfaces.Persistence;

namespace LogiCore.Application.Features.Vehicle.GetAllVehicles;

public class GetAllVehiclesHandler : IRequestHandler<GetAllVehiclesQuery, Result<IEnumerable<VehicleDto>>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IMapper _mapper;

    public GetAllVehiclesHandler(IVehicleRepository vehicleRepository, IMapper mapper)
    {
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<VehicleDto>>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleRepository.GetAllAsync();
        var dtos = vehicles.Select(v => _mapper.Map<VehicleDto>(v));
        return Result<IEnumerable<VehicleDto>>.Success(dtos);
    }
}
