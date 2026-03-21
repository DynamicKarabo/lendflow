using System;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;
using StackExchange.Redis;

namespace LendFlow.Infrastructure.Services;

public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisIdempotencyService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<string?> GetStoredResultAsync(string key)
    {
        var db = _redis.GetDatabase();
        var redisKey = $"lendflow:idempotency:{key}";
        var value = await db.StringGetAsync(redisKey);
        
        return value.HasValue ? value.ToString() : null;
    }

    public async Task StoreResultAsync(string key, string result)
    {
        var db = _redis.GetDatabase();
        var redisKey = $"lendflow:idempotency:{key}";
        await db.StringSetAsync(redisKey, result, TimeSpan.FromHours(24));
    }
}
