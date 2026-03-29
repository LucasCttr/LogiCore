using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LogiCore.Application.Features.Driver.Register;

public class RegisterDriverCommandHandler : IRequestHandler<RegisterDriverCommand, Result<DriverDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterDriverCommandHandler(UserManager<ApplicationUser> userManager, IDriverRepository driverRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _userManager = userManager;
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DriverDto>> Handle(RegisterDriverCommand request, CancellationToken cancellationToken)
    {
        // create application user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            var error = identityResult.Errors.FirstOrDefault()?.Description ?? "Error creating user";
            return Result<DriverDto>.Failure(error);
        }

        // create driver profile
        var fullName = $"{request.FirstName} {request.LastName}".Trim();
        var driver = LogiCore.Domain.Entities.Driver.Create(fullName, request.LicenseNumber);
        var added = await _driverRepository.AddAsync(driver);

        // commit both (user created through Identity stores, driver via unit of work)
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result<DriverDto>.Success(_mapper.Map<DriverDto>(added));
    }
}
