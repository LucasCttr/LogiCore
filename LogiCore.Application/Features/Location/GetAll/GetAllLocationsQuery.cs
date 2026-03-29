using System.Collections.Generic;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Location.GetAll;

public record GetAllLocationsQuery() : IRequest<Result<IEnumerable<LocationDto>>>;
