using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AutoMapper;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Application.DTOs;

namespace LogiCore.Application.Features.Location.CreateLocation;

public class CreateLocationHandler : IRequestHandler<CreateLocationCommand, Result<LocationDto>>
{
    private readonly ILocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateLocationHandler(ILocationRepository locationRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<LocationDto>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var location = LogiCore.Domain.Entities.Location.Create(request.Name, request.AddressLine1, request.AddressLine2, request.City, request.State, request.PostalCode, request.Country);
            var added = await _locationRepository.AddAsync(location);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result<LocationDto>.Success(_mapper.Map<LocationDto>(added));
        }
        catch (LogiCore.Domain.Common.Exceptions.DomainException ex)
        {
            return Result<LocationDto>.Failure(ex.Message);
        }
    }
}
