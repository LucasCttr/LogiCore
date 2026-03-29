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
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IDriverRepository _driverRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RegisterDriverCommandHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IDriverRepository driverRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _driverRepository = driverRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DriverDto>> Handle(RegisterDriverCommand request, CancellationToken cancellationToken)
    {
        // Start a transaction so creating the Identity user and Driver profile are atomic
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
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
                await transaction.RollbackAsync(cancellationToken);
                return Result<DriverDto>.Failure(error);
            }

            // ensure role exists and assign
            var roleName = "Driver";
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            await _userManager.AddToRoleAsync(user, roleName);

            // create driver profile linked to identity user (ApplicationUserId required)
            var fullName = $"{request.FirstName} {request.LastName}".Trim();
            var driver = LogiCore.Domain.Entities.Driver.Create(fullName, request.LicenseNumber, user.Id);
            var added = await _driverRepository.AddAsync(driver);

            // persist changes and commit transaction
            await _unitOfWork.CommitAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Result<DriverDto>.Success(_mapper.Map<DriverDto>(added));
        }
        catch (Exception)
        {
            try { await transaction.RollbackAsync(cancellationToken); } catch { }
            return Result<DriverDto>.Failure("Critical error while registering driver.");
        }
    }
}
