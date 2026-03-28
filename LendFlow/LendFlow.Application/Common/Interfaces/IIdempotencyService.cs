using System.Threading.Tasks;

namespace LendFlow.Application.Common.Interfaces;

public interface IIdempotencyService
{
    Task<string?> GetStoredResultAsync(string key);
    Task StoreResultAsync(string key, string result);
}
