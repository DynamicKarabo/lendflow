# ef-core-patterns.md

## Intent
Encode EF Core conventions for .NET 8 projects. Prevents LLMs from generating lazy,
incorrect, or performance-destroying EF Core code. Every pattern here exists because
the alternative has a concrete failure mode.

---

## DbContext Setup

```csharp
public class AppDbContext : DbContext
{
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _userService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService tenantService,
        ICurrentUserService userService) : base(options)
    {
        _tenantService = tenantService;
        _userService = userService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from assembly — never inline in OnModelCreating
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    // Audit fields handled here — not in handlers, not in repositories
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy = _userService.UserId;
            }
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy = _userService.UserId;
            }
        }
        return await base.SaveChangesAsync(ct);
    }
}
```

---

## Entity Configuration

- Every entity gets its own `IEntityTypeConfiguration<T>` class — never configure inline in `OnModelCreating`
- Table names explicit — never rely on EF convention naming in production code
- All column types explicit — never let EF guess `nvarchar(max)` vs `nvarchar(100)`

```csharp
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()   // Enum stored as string — readable in DB
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.IdempotencyKey)
            .HasMaxLength(255)
            .IsRequired();

        // Unique constraint on idempotency key per tenant
        builder.HasIndex(p => new { p.TenantId, p.IdempotencyKey })
            .IsUnique();
    }
}
```

---

## Multi-Tenancy via Global Query Filters

- Every tenant-scoped entity gets a global query filter — set once, applies everywhere
- Tenant ID resolved from `ICurrentTenantService` — injected into DbContext
- Never bypass with `IgnoreQueryFilters()` except in explicit admin/system contexts

```csharp
// In entity configuration
builder.HasQueryFilter(p => p.TenantId == _tenantService.TenantId);
```

**Rules:**
- `TenantId` is always a non-nullable `Guid` on tenant-scoped entities
- Global query filter = structural impossibility of cross-tenant data leakage
- If you need to bypass (admin reporting), document why explicitly in the code

---

## Query Patterns

### Never use lazy loading
```csharp
// WRONG — lazy loading hidden N+1
var payments = await _context.Payments.ToListAsync();
foreach (var p in payments)
    Console.WriteLine(p.Merchant.Name); // N+1 query per payment

// RIGHT — explicit include
var payments = await _context.Payments
    .Include(p => p.Merchant)
    .ToListAsync();
```

### Split reads from writes
- Commands: use EF Core with change tracking — you need it for saves
- Queries: use Dapper directly — no change tracking overhead, faster, explicit SQL

```csharp
// Query side — Dapper, no EF overhead
public async Task<PaymentDto?> GetByIdAsync(Guid id, Guid tenantId)
{
    const string sql = """
        SELECT p.Id, p.Amount, p.Currency, p.Status, p.CreatedAt
        FROM Payments p
        WHERE p.Id = @Id AND p.TenantId = @TenantId
        """;

    return await _connection.QuerySingleOrDefaultAsync<PaymentDto>(sql, new { Id = id, TenantId = tenantId });
}
```

### AsNoTracking for read-only EF queries
- If you must use EF for reads (simple cases), always use `AsNoTracking()`
- Never use EF with change tracking for queries that don't save

```csharp
var payment = await _context.Payments
    .AsNoTracking()
    .FirstOrDefaultAsync(p => p.Id == id, ct);
```

### Projection over full entity loads
```csharp
// WRONG — loads entire entity graph for a list view
var payments = await _context.Payments.Include(p => p.Merchant).ToListAsync();

// RIGHT — project to DTO, load only what you need
var payments = await _context.Payments
    .Select(p => new PaymentSummaryDto(p.Id, p.Amount, p.Currency, p.Status))
    .ToListAsync();
```

---

## Concurrency

### Optimistic concurrency with row version
- Use `rowversion` / `timestamp` column for optimistic concurrency on contested entities
- EF throws `DbUpdateConcurrencyException` on conflict — handle explicitly

```csharp
// In entity
public byte[] RowVersion { get; private set; }

// In configuration
builder.Property(p => p.RowVersion)
    .IsRowVersion()
    .IsConcurrencyToken();
```

### Handling concurrency exceptions
```csharp
try
{
    await _context.SaveChangesAsync(ct);
}
catch (DbUpdateConcurrencyException ex)
{
    // Reload and retry, or surface conflict to caller
    throw new ConflictException("Payment was modified by another process.");
}
```

---

## Migrations

- Migrations are code-first — never hand-edit a migration after it has been applied anywhere
- Always review generated migration before applying — EF sometimes generates destructive operations silently
- Never apply migrations in application startup (`Database.Migrate()`) in production — use MigrateCLI or deployment pipeline
- Migration names are descriptive: `AddIdempotencyKeyToPayments` not `Migration1`

```bash
# Always name migrations explicitly
dotnet ef migrations add AddIdempotencyKeyToPayments --project Infrastructure --startup-project Api
```

---

## Anti-Patterns

| Anti-Pattern | Why It's Wrong |
|---|---|
| Lazy loading enabled | Hides N+1 — EF will destroy performance silently |
| Inline entity config in `OnModelCreating` | Becomes unmaintainable fast |
| `nvarchar(max)` on everything | Poor index performance, wasted storage |
| Storing enums as integers | Unreadable in DB, painful to debug |
| `Database.Migrate()` in startup | Dangerous in multi-instance deployments |
| EF for read queries with change tracking | Unnecessary overhead — use Dapper |
| Missing `AsNoTracking()` on EF reads | Change tracker holds objects in memory unnecessarily |
| Not reviewing generated migrations | EF silently generates DROP COLUMN on renames |
| Raw SQL without tenant filter | Bypasses global query filter — cross-tenant leak risk |

---

## Checklist

- [ ] Entity has its own `IEntityTypeConfiguration<T>` class
- [ ] All column types explicit — no EF convention guessing
- [ ] Enums stored as strings
- [ ] Tenant-scoped entities have global query filter
- [ ] No lazy loading — `UseLazyLoadingProxies` never called
- [ ] Read queries use Dapper or `AsNoTracking()`
- [ ] List queries project to DTOs — not full entity loads
- [ ] Contested entities have `rowversion` concurrency token
- [ ] Migration reviewed before applying — no silent destructive ops
- [ ] Migration name is descriptive
