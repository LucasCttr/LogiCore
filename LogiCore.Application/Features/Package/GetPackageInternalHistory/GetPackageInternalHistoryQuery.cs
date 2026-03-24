using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Package.GetPackageHistory;

public record GetPackageHistoryQuery(Guid PackageId) : IRequest<Result<IEnumerable<PackageInternalHistoryDto>>>;
