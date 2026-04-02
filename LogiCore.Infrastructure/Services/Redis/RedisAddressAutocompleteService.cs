using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using LogiCore.Application.Common.Interfaces;

namespace LogiCore.Infrastructure.Services.Redis
{
    public class RedisAddressAutocompleteService : IAddressAutocompleteRepository
    {
        private readonly IConnectionMultiplexer _conn;
        private readonly IDatabase _db;
        private const string Key = "addresses:zset";
        private const string UsageKey = "addresses:usage";

        public RedisAddressAutocompleteService(IConnectionMultiplexer conn)
        {
            _conn = conn;
            _db = _conn.GetDatabase();
        }

        public Task AddAddressAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address)) return Task.CompletedTask;
            var a = address.Trim();
            // add to lex zset (score is irrelevant for lex operations)
            var addLex = _db.SortedSetAddAsync(Key, a, 0);
            // ensure usage zset has an entry with initial score 0
            var addUsage = _db.SortedSetAddAsync(UsageKey, a, 0);
            return Task.WhenAll(addLex, addUsage);
        }

        public async Task<IEnumerable<string>> GetSuggestionsAsync(string prefix, int limit = 5)
        {
            if (prefix == null) prefix = string.Empty;
            var min = prefix;
            var max = prefix + "\uffff";
            // Fetch candidate set by lex range (take more candidates to allow ordering by score)
            var candidateTake = Math.Max(limit * 10, 50);
            var candidates = (await _db.SortedSetRangeByValueAsync(Key, min: min, max: max, exclude: StackExchange.Redis.Exclude.None, take: candidateTake)).Select(v => v.ToString()).ToArray();
            if (candidates.Length == 0) return Enumerable.Empty<string>();

            // Fetch scores in parallel
            var scoreTasks = candidates.Select(c => _db.SortedSetScoreAsync(UsageKey, c)).ToArray();
            await Task.WhenAll(scoreTasks);
            var scored = candidates.Select((c, i) => new { Address = c, Score = scoreTasks[i].Result ?? 0 })
                                   .OrderByDescending(x => x.Score)
                                   .ThenBy(x => x.Address)
                                   .Take(limit)
                                   .Select(x => x.Address);

            return scored;
        }

        public async Task SeedAsync(IEnumerable<string> addresses)
        {
            if (addresses == null) return;
            var entries = addresses
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(a => new SortedSetEntry(a.Trim(), 0))
                .ToArray();
            if (entries.Length == 0) return;
            await _db.SortedSetAddAsync(Key, entries);
            // seed usage zset with same members and score 0
            var usageEntries = entries.Select(e => new SortedSetEntry(e.Element, 0)).ToArray();
            await _db.SortedSetAddAsync(UsageKey, usageEntries);
        }

        public Task IncrementScoreAsync(string address, double increment = 1)
        {
            if (string.IsNullOrWhiteSpace(address)) return Task.CompletedTask;
            return _db.SortedSetIncrementAsync(UsageKey, address.Trim(), increment);
        }
    }
}
