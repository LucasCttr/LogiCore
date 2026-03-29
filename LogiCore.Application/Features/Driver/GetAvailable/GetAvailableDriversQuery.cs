using System.Collections.Generic;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Driver.GetAvailable;

public record GetAvailableDriversQuery() : IRequest<Result<IEnumerable<DriverDto>>>;
