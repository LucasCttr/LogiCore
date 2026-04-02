using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Entities;
using LogiCore.Application.DTOs;
using AutoMapper;
using LogiCore.Application.Common.Interfaces.Security;
using LogiCore.Domain.ValueObjects;

namespace LogiCore.Application.Features.Packages;
public class CreatePackageCommandHandler : IRequestHandler<CreatePackageCommand, Result<PackageDto>>
{
    private readonly IPackageRepository _packageRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly LogiCore.Application.Services.IAddressAutocompleteService _autocompleteService;

    public CreatePackageCommandHandler(IPackageRepository packageRepository, IMapper mapper, ICurrentUserService currentUserService, LogiCore.Application.Services.IAddressAutocompleteService autocompleteService)
    {
        _packageRepository = packageRepository;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _autocompleteService = autocompleteService;
    }

    public async Task<Result<PackageDto>> Handle(CreatePackageCommand request, CancellationToken cancellationToken)
    {
        // Business validation: ensure tracking number is unique
        if (await _packageRepository.ExistsByTrackingNumberAsync(request.TrackingNumber))
        {
            return Result<PackageDto>.Failure("A package with the same tracking number already exists.", ErrorType.Conflict);
        }

        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Result<PackageDto>.Failure("Unauthorized", ErrorType.Unauthorized);
        // Build recipient from provided fields. If legacy recipient fields are missing, use Destination as address.
        var recipientName = string.IsNullOrWhiteSpace(request.RecipientName) ? "Unknown" : request.RecipientName!;
        var recipientAddress = !string.IsNullOrWhiteSpace(request.RecipientAddress) ? request.RecipientAddress! : (request.Destination ?? string.Empty);
        var recipientPhone = request.RecipientPhone ?? string.Empty;
        var recipientFloor = request.RecipientFloorApartment;
        var recipientCity = request.RecipientCity ?? string.Empty;
        var recipientProvince = request.RecipientProvince ?? string.Empty;
        var recipientPostal = request.RecipientPostalCode ?? string.Empty;
        var recipientDni = request.RecipientDni ?? string.Empty;

        var recipient = Recipient.Create(recipientName, recipientAddress, recipientPhone, recipientFloor, recipientCity, recipientProvince, recipientPostal, recipientDni);

        // Dimensions: use provided values if present and valid, otherwise use small default dimensions.
        LogiCore.Domain.ValueObjects.Dimensions dims;
        if (request.LengthCm.HasValue && request.WidthCm.HasValue && request.HeightCm.HasValue
            && request.LengthCm.Value > 0 && request.WidthCm.Value > 0 && request.HeightCm.Value > 0)
        {
            dims = LogiCore.Domain.ValueObjects.Dimensions.Create(request.LengthCm.Value, request.WidthCm.Value, request.HeightCm.Value);
        }
        else
        {
            dims = LogiCore.Domain.ValueObjects.Dimensions.Create(1m, 1m, 1m);
        }

        var weight = request.Weight.HasValue && request.Weight.Value > 0 ? request.Weight.Value : 0.1m;
        var package = Domain.Entities.Package.Create(request.TrackingNumber ?? string.Empty, recipient, weight, userId, dims);
        var added = await _packageRepository.AddAsync(package);
        // Also register this address with the autocomplete service so it appears in suggestions
        if (!string.IsNullOrWhiteSpace(recipientAddress))
        {
            await _autocompleteService.AddAddressAsync(recipientAddress);
            await _autocompleteService.RecordSelectionAsync(recipientAddress);
        }

        // Do not call SaveChanges here; SaveChanges will be executed by the SaveChangesBehavior (UnitOfWork) after handler completes
        return Result<PackageDto>.Success(_mapper.Map<PackageDto>(added));
    }
}