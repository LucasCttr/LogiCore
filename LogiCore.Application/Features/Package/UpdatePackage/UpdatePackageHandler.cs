using System;
using System.Collections.Generic;
using System.Linq;
using System;
using MediatR;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;
using LogiCore.Application.Common.Interfaces.Persistence;
using AutoMapper;


namespace LogiCore.Application.Features.Packages
{
    public class UpdatePackageCommandHandler : IRequestHandler<UpdatePackageCommand, Result<PackageDto>> 
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IMapper _mapper;

        public UpdatePackageCommandHandler(IPackageRepository packageRepository, IMapper mapper)
        {
            _packageRepository = packageRepository;
            _mapper = mapper;
        }

        public async Task<Result<PackageDto>> Handle(UpdatePackageCommand request, CancellationToken cancellationToken)
        {
            var existingPackage = await _packageRepository.GetByIdAsync(request.Id);
            if (existingPackage == null)
            {
                return Result<PackageDto>.Failure("Package not found.");
            }

            // Update fields via entity methods to respect encapsulation
            if (!string.IsNullOrEmpty(request.TrackingNumber))
                existingPackage.UpdateTrackingNumber(request.TrackingNumber);

            if (!string.IsNullOrEmpty(request.RecipientName))
            {
                var recipient = LogiCore.Domain.ValueObjects.Recipient.Create(request.RecipientName, request.RecipientAddress, request.RecipientPhone);
                existingPackage.UpdateRecipient(recipient);
            }

            if (request.Weight.HasValue)
                existingPackage.UpdateWeight(request.Weight.Value);

            var updatedPackage = await _packageRepository.UpdateAsync(existingPackage);

            var updatedDto = _mapper.Map<PackageDto>(updatedPackage);

            return Result<PackageDto>.Success(updatedDto);
        }

    }
}