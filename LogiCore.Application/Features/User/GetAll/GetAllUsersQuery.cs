using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.User.GetAll;

public record GetAllUsersQuery(int PageNumber = 1, int PageSize = 15) : IRequest<Result<PagedResult<LogiCore.Application.DTOs.UserDto>>> { }
