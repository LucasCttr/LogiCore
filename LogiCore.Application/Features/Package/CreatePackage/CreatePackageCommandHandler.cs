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
        // Generate tracking number if not provided
        var trackingNumber = request.TrackingNumber;
        if (string.IsNullOrWhiteSpace(trackingNumber))
        {
            trackingNumber = await GenerateUniqueTrackingNumberAsync();
        }
        else
        {
            // Business validation: ensure tracking number is unique if provided
            if (await _packageRepository.ExistsByTrackingNumberAsync(trackingNumber))
            {
                return Result<PackageDto>.Failure("A package with the same tracking number already exists.", ErrorType.Conflict);
            }
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

        // Dimensions: use provided values if valid, otherwise use small default dimensions.
        LogiCore.Domain.ValueObjects.Dimensions dims;
        if (request.LengthCm > 0 && request.WidthCm > 0 && request.HeightCm > 0)
        {
            dims = LogiCore.Domain.ValueObjects.Dimensions.Create(request.LengthCm, request.WidthCm, request.HeightCm);
        }
        else
        {
            dims = LogiCore.Domain.ValueObjects.Dimensions.Create(1m, 1m, 1m);
        }

        var weight = request.Weight > 0 ? request.Weight : 0.1m;
        var package = Domain.Entities.Package.Create(
            trackingNumber,
            recipient,
            weight,
            userId,
            dims,
            request.Description,
            request.InternalCode,
            request.Origin,
            request.Destination);
        var added = await _packageRepository.AddAsync(package);
        // Also register this address with the autocomplete service so it appears in suggestions
        if (!string.IsNullOrWhiteSpace(recipientAddress))
        {
            await _autocompleteService.RecordSelectionAsync(recipientAddress);
        }
        // Record origin/destination addresses in autocomplete so they appear in suggestions
        if (!string.IsNullOrWhiteSpace(request.Origin))
        {
            await _autocompleteService.RecordSelectionAsync(request.Origin);
        }

        if (!string.IsNullOrWhiteSpace(request.Destination))
        {
            await _autocompleteService.RecordSelectionAsync(request.Destination);
        }

        // Do not call SaveChanges here; SaveChanges will be executed by the SaveChangesBehavior (UnitOfWork) after handler completes
        return Result<PackageDto>.Success(_mapper.Map<PackageDto>(added));
    }

    private async Task<string> GenerateUniqueTrackingNumberAsync()
    {
        string trackingNumber;
        const int maxAttempts = 10;
        int attempts = 0;

        do
        {
            trackingNumber = $"PKG-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
            attempts++;
        } while (await _packageRepository.ExistsByTrackingNumberAsync(trackingNumber) && attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            throw new InvalidOperationException("Unable to generate unique tracking number after maximum attempts.");
        }

        return trackingNumber;
    }
}