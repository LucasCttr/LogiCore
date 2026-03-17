using MediatR;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Application.Common.Models;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.Features.Packages;
public class CreatePackageCommandHandler : IRequestHandler<CreatePackageCommand, Result<Guid>>
{
    private readonly IPackageRepository _packageRepository;

    public CreatePackageCommandHandler(IPackageRepository packageRepository)
    {
        _packageRepository = packageRepository;
    }

    public async Task<Result<Guid>> Handle(CreatePackageCommand request, CancellationToken cancellationToken)
    {
        var package = Package.Create(request.TrackingNumber, request.RecipientName, request.Weight);
        await _packageRepository.AddAsync(package);
        await _packageRepository.SaveChangesAsync();
        return Result<Guid>.Success(package.Id);
    }
}