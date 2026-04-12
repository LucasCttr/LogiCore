using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Driver.Update;

public class UpdateDriverHandler : IRequestHandler<UpdateDriverCommand, Result<DriverDto>>
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateDriverHandler(
        IDriverRepository driverRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DriverDto>> Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get driver with ApplicationUser
            var driver = await _driverRepository.GetByIdAsync(request.DriverId);
            if (driver == null)
                return Result<DriverDto>.Failure("Driver not found.", ErrorType.NotFound);

            // Update driver name and license
            var fullName = $"{request.FirstName} {request.LastName}".Trim();
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                driver.SetName(fullName);
            }

            if (!string.IsNullOrWhiteSpace(request.LicenseNumber))
            {
                driver.SetLicenseNumber(request.LicenseNumber);
            }

            // Update user email and phone if ApplicationUser is loaded
            if (driver.ApplicationUser != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    driver.ApplicationUser.Email = request.Email;
                    driver.ApplicationUser.UserName = request.Email;
                    driver.ApplicationUser.NormalizedEmail = request.Email.ToUpper();
                    driver.ApplicationUser.NormalizedUserName = request.Email.ToUpper();
                }

                if (!string.IsNullOrWhiteSpace(request.Phone))
                {
                    driver.ApplicationUser.PhoneNumber = request.Phone;
                }
            }

            // Save changes
            await _driverRepository.UpdateAsync(driver);
            await _unitOfWork.CommitAsync(cancellationToken);

            // Return updated driver
            var result = await _driverRepository.GetByIdAsync(request.DriverId);
            var dto = _mapper.Map<DriverDto>(result);
            return Result<DriverDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<DriverDto>.Failure($"Error updating driver: {ex.Message}");
        }
    }
}
