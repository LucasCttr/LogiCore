using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Features.User.GetAll;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var allUsers = _userManager.Users.ToList();
            var totalCount = allUsers.Count;

            // Apply pagination
            var paginatedUsers = allUsers
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTOs and get roles
            var userDtos = new List<UserDto>();
            foreach (var user in paginatedUsers)
            {
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
                userDtos.Add(userDto);
            }

            var result = new PagedResult<UserDto>(
                Items: userDtos,
                Total: totalCount,
                Page: request.PageNumber,
                PageSize: request.PageSize
            );

            return Result<PagedResult<UserDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<UserDto>>.Failure($"Error retrieving users: {ex.Message}", ErrorType.Validation);
        }
    }
}
