using LogiCore.Application.Common.Models;
using MediatR;

namespace LogiCore.Application.Features.Driver.GetAll;

public class GetAllDriversQuery : IRequest<Result<IEnumerable<LogiCore.Application.DTOs.DriverDto>>> { }
