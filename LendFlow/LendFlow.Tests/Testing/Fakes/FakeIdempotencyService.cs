using System.Collections.Concurrent;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;

namespace LendFlow.Tests.Testing.Fakes;

public class FakeIdempotencyService : IIdempotencyService
{
    private readonly ConcurrentDictionary<string, string> _store = new();

    public Task<string?> GetStoredResultAsync(string key)
    {
        return Task.FromResult(_store.TryGetValue(key, out var value) ? value : null);
    }

    public Task StoreResultAsync(string key, string result)
    {
        _store[key] = result;
        return Task.CompletedTask;
    }
}
