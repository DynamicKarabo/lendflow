# redis-patterns.md

## Intent
Encode Redis conventions for .NET 8 projects covering caching strategy, key design,
idempotency, TTL conventions, and failure modes. LLMs treat Redis as a simple key-value
store and miss the failure modes that destroy correctness in production.

---

## Connection Setup

```csharp
// Always use StackExchange.Redis — not the Microsoft.Extensions.Caching.StackExchangeRedis abstraction
// for anything beyond simple caching. You need direct control.
services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));

services.AddScoped<IDatabase>(sp =>
    sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
```

**Rules:**
- `IConnectionMultiplexer` is singleton — one connection pool per app instance
- `IDatabase` is cheap — resolve per scope, not singleton
- Always configure `abortConnect=false` in connection string — app starts even if Redis is temporarily unavailable
- Always configure `connectRetry` and `connectTimeout`

```
// Connection string
redis:6379,abortConnect=false,connectRetry=3,connectTimeout=5000
```

---

## Key Design

Keys are your schema in Redis. Bad key design = impossible to manage, debug, or expire correctly.

### Convention
```
{app}:{domain}:{entity}:{id}:{variant}

Examples:
payflow:idempotency:payment:{idempotencyKey}
payflow:cache:merchant:{merchantId}:profile
payflow:session:{sessionId}
payflow:ratelimit:{merchantId}:{window}
```

**Rules:**
- Always namespace with app name first — shared Redis clusters need isolation
- Use colons as separators — Redis convention, tooling respects it
- Never use spaces or special characters in keys
- Keep keys human-readable — you will be debugging at 2am
- Document key patterns in this file when you add them

---

## TTL Strategy

Every key must have a TTL. Keys without TTL are memory leaks.

| Pattern | Recommended TTL | Why |
|---|---|---|
| Idempotency keys | 24–48 hours | Long enough to cover retry windows |
| Cache (user/merchant profile) | 5–15 minutes | Balance freshness vs DB load |
| Cache (reference data, rarely changes) | 1–24 hours | Config, fee tiers, etc. |
| Rate limit windows | Duration of window | Auto-expires with window |
| Session tokens | Session lifetime | Match auth token expiry |
| Distributed locks | Operation timeout + buffer | Never leave locks orphaned |

```csharp
// Always set TTL — never call StringSetAsync without expiry
await _db.StringSetAsync(key, value, TimeSpan.FromHours(24));
```

---

## Idempotency Pattern

This is the most critical Redis pattern in fintech. Atomic check-and-set with SET NX.

```csharp
public async Task<IdempotencyResult> AcquireAsync(string key, TimeSpan ttl)
{
    // SET NX — atomic. Either you set it or you don't. No race condition.
    var acquired = await _db.StringSetAsync(
        key: $"payflow:idempotency:{key}",
        value: "processing",
        expiry: ttl,
        when: When.NotExists   // NX — only set if not exists
    );

    return new IdempotencyResult(acquired);
}

public async Task StoreResultAsync(string key, string result, TimeSpan ttl)
{
    // Replace "processing" sentinel with actual result
    await _db.StringSetAsync(
        key: $"payflow:idempotency:{key}",
        value: result,
        expiry: ttl
    );
}

public async Task<string?> GetStoredResultAsync(string key)
{
    return await _db.StringGetAsync($"payflow:idempotency:{key}");
}
```

**Flow:**
1. `AcquireAsync` — returns `acquired: true` if first caller, `false` if duplicate
2. If duplicate: `GetStoredResultAsync` — return stored result (may be "processing" if still in flight)
3. After successful processing: `StoreResultAsync` — replace sentinel with real result

---

## Caching Pattern

### Cache-aside (lazy loading)
```csharp
public async Task<MerchantProfile?> GetMerchantAsync(Guid merchantId, CancellationToken ct)
{
    var cacheKey = $"payflow:cache:merchant:{merchantId}:profile";

    // 1. Check cache
    var cached = await _db.StringGetAsync(cacheKey);
    if (cached.HasValue)
        return JsonSerializer.Deserialize<MerchantProfile>(cached!);

    // 2. Cache miss — load from DB
    var merchant = await _repository.GetByIdAsync(merchantId, ct);
    if (merchant is null) return null;

    // 3. Populate cache
    await _db.StringSetAsync(
        cacheKey,
        JsonSerializer.Serialize(merchant),
        TimeSpan.FromMinutes(10)
    );

    return merchant;
}
```

### Cache invalidation
```csharp
// Invalidate on mutation — delete the key, don't try to update it
public async Task UpdateMerchantAsync(UpdateMerchantCommand command, CancellationToken ct)
{
    await _repository.UpdateAsync(command, ct);

    // Invalidate cache — next read will repopulate
    await _db.KeyDeleteAsync($"payflow:cache:merchant:{command.MerchantId}:profile");
}
```

**Rules:**
- Never try to update cache entries in-place — delete and let next read repopulate
- Cache invalidation on every write path that touches the cached entity
- If cache miss rate is high, the TTL is too short — tune based on actual read patterns

---

## Distributed Locking

Use when you need exactly one process doing something at a time (settlement runs, batch jobs).

```csharp
public async Task<IDisposable?> AcquireLockAsync(string resource, TimeSpan expiry)
{
    var lockKey = $"payflow:lock:{resource}";
    var lockValue = Guid.NewGuid().ToString(); // Unique per caller — prevents releasing someone else's lock

    var acquired = await _db.StringSetAsync(lockKey, lockValue, expiry, When.NotExists);
    if (!acquired) return null;

    return new RedisLock(_db, lockKey, lockValue);
}

// Usage
var lock = await _lockService.AcquireLockAsync("settlement-run", TimeSpan.FromMinutes(5));
if (lock is null)
{
    _logger.LogWarning("Settlement run already in progress — skipping");
    return;
}
await using (lock)
{
    await RunSettlementAsync();
}
```

**Rules:**
- Lock value must be unique per caller — prevents a slow process releasing a lock acquired by someone else
- Lock TTL must exceed worst-case operation duration + buffer
- If operation may exceed TTL, extend the lock (Redlock pattern) — don't just set a high TTL

---

## Rate Limiting Pattern

```csharp
public async Task<bool> IsAllowedAsync(string merchantId, int maxRequests, TimeSpan window)
{
    var key = $"payflow:ratelimit:{merchantId}:{window.TotalSeconds}";

    var count = await _db.StringIncrementAsync(key);

    if (count == 1)
    {
        // First request in window — set TTL
        await _db.KeyExpireAsync(key, window);
    }

    return count <= maxRequests;
}
```

---

## Failure Modes

### Redis unavailability
- Redis going down should NOT take your app down for non-critical caching
- Wrap cache reads in try/catch — fall through to DB on Redis failure
- Idempotency and locking are different — these ARE critical, surface the failure

```csharp
// Non-critical cache — degrade gracefully
try
{
    var cached = await _db.StringGetAsync(cacheKey);
    if (cached.HasValue) return Deserialize(cached);
}
catch (RedisException ex)
{
    _logger.LogWarning(ex, "Redis unavailable — falling through to DB");
}
return await _repository.GetAsync(id, ct);
```

### Thundering herd
- Cache expiry causes all requests to hit DB simultaneously
- Mitigate with jitter on TTL: `BaseExpiry + Random(0, JitterSeconds)`

```csharp
var ttl = TimeSpan.FromMinutes(10) + TimeSpan.FromSeconds(Random.Shared.Next(0, 60));
await _db.StringSetAsync(key, value, ttl);
```

---

## Anti-Patterns

| Anti-Pattern | Why It's Wrong |
|---|---|
| Keys without TTL | Memory leak — Redis evicts unpredictably under pressure |
| Non-namespaced keys | Collisions on shared clusters |
| Updating cache in-place | Stale data risk — delete and repopulate |
| Non-atomic idempotency check | GET then SET = race condition, double processing |
| Catching all Redis exceptions on idempotency | Silent correctness failure |
| Lock value not unique per caller | Slow process releases another caller's lock |
| No jitter on cache TTL | Thundering herd on expiry |
| Storing large objects in Redis | Serialization overhead, memory pressure — keep values small |

---

## Key Registry

Document every key pattern used in the project here:

| Key Pattern | TTL | Purpose |
|---|---|---|
| `payflow:idempotency:payment:{key}` | 24h | Payment idempotency |
| `payflow:cache:merchant:{id}:profile` | 10min | Merchant profile cache |
| `payflow:lock:settlement-run` | 5min | Settlement run distributed lock |
| `payflow:ratelimit:{merchantId}:{window}` | Window duration | API rate limiting |

---

## Checklist

- [ ] All keys namespaced: `{app}:{domain}:{entity}:{id}`
- [ ] All keys have TTL — no naked `StringSetAsync` without expiry
- [ ] Idempotency uses SET NX — not GET then SET
- [ ] Lock values are unique per caller (GUID)
- [ ] Lock TTL exceeds worst-case operation duration
- [ ] Cache invalidation on every write path
- [ ] TTL has jitter for high-traffic cached entities
- [ ] Non-critical cache reads degrade gracefully on Redis failure
- [ ] Critical Redis operations (idempotency, locks) surface failures — not swallowed
- [ ] New key patterns documented in Key Registry above
