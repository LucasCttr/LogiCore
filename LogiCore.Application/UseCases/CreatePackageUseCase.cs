using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogiCore.Application.Commands;
using LogiCore.Application.Common.Interfaces.Persistence;
using LogiCore.Domain.Entities;

namespace LogiCore.Application.UseCases
{
    public class CreatePackageUseCase
    {
        private readonly IPackageRepository _packageRepository;

        public CreatePackageUseCase(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }

        public async Task ExecuteAsync(CreatePackageCommand cmd)
        {
            var package = new Package(cmd.TrackingNumber, cmd.RecipientName, cmd.Weight);
            await _packageRepository.AddAsync(package);
            await _packageRepository.SaveChangesAsync();
        }
    }
}