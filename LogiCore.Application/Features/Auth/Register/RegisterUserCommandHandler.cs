using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.DTOs;
using LogiCore.Application.Repositories;
using LogiCore.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LogiCore.Application.Features.Auth;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _modelMapper;
    private readonly IDriverDetailsRepository _driverDetailsRepository;
    private readonly IDriverRepository _driverRepository;

    public RegisterUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        IMapper modelMapper,
        IDriverDetailsRepository driverDetailsRepository,
        IDriverRepository driverRepository)
    {
        _userManager = userManager;
        _modelMapper = modelMapper;
        _driverDetailsRepository = driverDetailsRepository;
        _driverRepository = driverRepository;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // 1. Mapping command to entity
        var user = _modelMapper.Map<ApplicationUser>(request);

        // 2. Create user with password
        var result = await _userManager.CreateAsync(user, request.Password);

        // 3. Check if creation was successful
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault()?.Description ?? "Error creating user";
            return Result<UserDto>.Failure(error);
        }

        // 4. Assign roles if provided
        if (request.Roles != null && request.Roles.Any())
        {
            var rolesResult = await _userManager.AddToRolesAsync(user, request.Roles);
            if (!rolesResult.Succeeded)
            {
                var error = rolesResult.Errors.FirstOrDefault()?.Description ?? "Error assigning roles";
                return Result<UserDto>.Failure(error);
            }

            // 5. If "Driver" role is assigned, create DriverDetails and Driver records
            if (request.Roles.Contains("Driver"))
            {
                try
                {
                    // Create DriverDetails
                    var driverDetails = DriverDetails.Create(
                        userId: user.Id,
                        licenseNumber: "TBD",
                        licenseType: "TBD",
                        licenseExpiry: DateTime.UtcNow.AddYears(5),
                        insuranceExpiry: DateTime.UtcNow.AddYears(1)
                    );

                    await _driverDetailsRepository.CreateAsync(driverDetails);

                    // Create Driver entity
                    var fullName = $"{user.FirstName} {user.LastName}".Trim();
                    var driver = LogiCore.Domain.Entities.Driver.Create(
                        name: fullName,
                        licenseNumber: "TBD",
                        applicationUserId: user.Id
                    );

                    await _driverRepository.AddAsync(driver);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail user creation
                    Console.WriteLine($"Error creating DriverDetails or Driver: {ex.Message}");
                }
            }
        }

        // 6. Fetch roles and map to output DTO with roles included
        var roles = await _userManager.GetRolesAsync(user);
        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            IsActive = !user.LockoutEnd.HasValue || user.LockoutEnd <= DateTimeOffset.UtcNow,
            Roles = roles,
            CreatedAt = user.CreatedAt
        };

        return Result<UserDto>.Success(userDto);
    }
}