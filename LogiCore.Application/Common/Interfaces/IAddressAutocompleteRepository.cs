using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogiCore.Application.Common.Interfaces
{
    public interface IAddressAutocompleteRepository
    {
        Task AddAddressAsync(string address);
        Task<IEnumerable<string>> GetSuggestionsAsync(string prefix, int limit = 5);
        Task SeedAsync(IEnumerable<string> addresses);
        Task IncrementScoreAsync(string address, double increment = 1);
    }
}
