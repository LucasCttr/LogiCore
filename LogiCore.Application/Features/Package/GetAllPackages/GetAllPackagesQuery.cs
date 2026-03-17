using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Package;

public record GetAllPackagesQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResponse<PackageDto>>;