using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Driver.UpdateStatus;

public class UpdateDriverStatusHandler : IRequestHandler<UpdateDriverStatusCommand, Result<DriverDto>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateDriverStatusHandler(IDriverRepository driverRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DriverDto>> Handle(UpdateDriverStatusCommand request, CancellationToken cancellationToken)
    {
        var driver = await _driverRepository.GetByIdAsync(request.DriverId);
        if (driver == null) return Result<DriverDto>.Failure("Driver not found.");

        driver.SetActive(request.IsActive);
        await _driverRepository.UpdateAsync(driver);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<DriverDto>.Success(_mapper.Map<DriverDto>(driver));
    }
}
