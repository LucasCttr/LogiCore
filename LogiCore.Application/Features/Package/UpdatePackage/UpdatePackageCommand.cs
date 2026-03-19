using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using MediatR;

namespace LogiCore.Application.Features.Packages
{
    public record UpdatePackageCommand : IRequest<Result<PackageDto>>
    {
        public Guid Id { get; init; }
        public string? TrackingNumber { get; init; }
        public string? RecipientName { get; init; }
        public decimal? Weight { get; init; }
    }
}