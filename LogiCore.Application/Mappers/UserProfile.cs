using AutoMapper;
using LogiCore.Application.Features.Auth;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Mappers;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Mapping between ApplicationUser and UserDto (both directions if needed)
        CreateMap<ApplicationUser, UserDto>();

        // Mapping from RegisterUserCommand to ApplicationUser for user creation
        CreateMap<RegisterUserCommand, ApplicationUser>()
            // Map the Email to UserName as well, since IdentityUser requires UserName
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
    }
}

