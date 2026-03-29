using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Driver.GetByUser;

public class GetDriverByUserHandler : IRequestHandler<GetDriverByUserQuery, Result<DriverDto>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IMapper _mapper;

    public GetDriverByUserHandler(IDriverRepository driverRepository, IMapper mapper)
    {
        _driverRepository = driverRepository;
        _mapper = mapper;
    }

    public async Task<Result<DriverDto>> Handle(GetDriverByUserQuery request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByApplicationUserIdAsync(request.ApplicationUserId);
        if (driver == null) return Result<DriverDto>.Failure("Driver not found.", ErrorType.NotFound);
        return Result<DriverDto>.Success(_mapper.Map<DriverDto>(driver));
    }
}
