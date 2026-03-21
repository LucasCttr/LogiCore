using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Entities;
using LogiCore.Application.DTOs;
using AutoMapper;
using LogiCore.Application.Common.Interfaces.Security;

namespace LogiCore.Application.Features.Packages;
public class CreatePackageCommandHandler : IRequestHandler<CreatePackageCommand, Result<PackageDto>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IMapper _mapper;
    private readonly LogiCore.Application.Common.Interfaces.Security.ICurrentUserService _currentUserService;

    public CreatePackageCommandHandler(IPackageRepository packageRepository, IMapper mapper, ICurrentUserService currentUserService)
    {
        _packageRepository = packageRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PackageDto>> Handle(CreatePackageCommand request, CancellationToken cancellationToken)
    {
        // Business validation: ensure tracking number is unique
        if (await _packageRepository.ExistsByTrackingNumberAsync(request.TrackingNumber))
        {
            return Result<PackageDto>.Failure("A package with the same tracking number already exists.");
        }

        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<PackageDto>.Failure("Unauthorized");

        var recipient = LogiCore.Domain.ValueObjects.Recipient.Create(request.RecipientName, request.RecipientAddress, request.RecipientPhone);
        var dims = LogiCore.Domain.ValueObjects.Dimensions.Create(request.LengthCm, request.WidthCm, request.HeightCm);
        var package = Package.Create(request.TrackingNumber, recipient, request.Weight, userId, dims);
        var added = await _packageRepository.AddAsync(package);
        // Do not call SaveChanges here; SaveChanges will be executed by the SaveChangesBehavior (UnitOfWork) after handler completes
        return Result<PackageDto>.Success(_mapper.Map<PackageDto>(added));
    }
}