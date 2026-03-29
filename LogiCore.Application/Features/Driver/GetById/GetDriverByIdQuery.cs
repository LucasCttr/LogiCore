using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Driver.GetById;

public class GetDriverByIdQuery : IRequest<Result<LogiCore.Application.DTOs.DriverDto>>
{
    public Guid Id { get; set; }
    public GetDriverByIdQuery(Guid id) => Id = id;
}
