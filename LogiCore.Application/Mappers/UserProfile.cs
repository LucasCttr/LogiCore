using AutoMapper;
using LogiCore.Application.Features.Auth;
using LogiCore.Application.DTOs;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Mappers;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Mapping between ApplicationUser and UserDto
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName ?? string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? string.Empty))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => !src.LockoutEnd.HasValue || src.LockoutEnd <= DateTimeOffset.UtcNow))
            .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Roles will be set by the handler

        // Mapping from RegisterUserCommand to ApplicationUser for user creation
        CreateMap<RegisterUserCommand, ApplicationUser>()
            // Map the Email to UserName as well, since IdentityUser requires UserName
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

        // Mapping for DriverDetails
        CreateMap<DriverDetails, DriverDetailsDto>();
    }
}


