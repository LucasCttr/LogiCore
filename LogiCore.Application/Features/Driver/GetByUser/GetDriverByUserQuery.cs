using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Driver.GetByUser;

public class GetDriverByUserQuery : IRequest<Result<LogiCore.Application.DTOs.DriverDto>>
{
    public string ApplicationUserId { get; }
    public GetDriverByUserQuery(string applicationUserId) => ApplicationUserId = applicationUserId;
}
