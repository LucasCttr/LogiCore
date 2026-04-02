using System.Collections.Generic;
using System.Threading.Tasks;
using LogiCore.Application.Common.Interfaces;

namespace LogiCore.Application.Services
{
    public interface IAddressAutocompleteService
    {
        Task<IEnumerable<string>> GetSuggestionsAsync(string prefix, int limit = 5);
        Task RecordSelectionAsync(string address);
        Task AddAddressAsync(string address);
    }

    public class AddressAutocompleteService : IAddressAutocompleteService
    {
        private readonly IAddressAutocompleteRepository _repo;

        public AddressAutocompleteService(IAddressAutocompleteRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<string>> GetSuggestionsAsync(string prefix, int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(prefix) || prefix.Trim().Length < 3)
                return new List<string>();

            return await _repo.GetSuggestionsAsync(prefix.Trim(), limit);
        }

        public Task RecordSelectionAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return Task.CompletedTask;
            return _repo.IncrementScoreAsync(address.Trim(), 1);
        }

        public Task AddAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return Task.CompletedTask;
            return _repo.AddAddressAsync(address.Trim());
        }
    }
}
