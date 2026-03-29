using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Driver.GetById;

public class GetDriverByIdHandler : IRequestHandler<GetDriverByIdQuery, Result<DriverDto>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IMapper _mapper;

    public GetDriverByIdHandler(IDriverRepository driverRepository, IMapper mapper)
    {
        _driverRepository = driverRepository;
        _mapper = mapper;
    }

    public async Task<Result<DriverDto>> Handle(GetDriverByIdQuery request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByIdAsync(request.Id);
        if (driver == null) return Result<DriverDto>.Failure("Driver not found.");
        return Result<DriverDto>.Success(_mapper.Map<DriverDto>(driver));
    }
}
