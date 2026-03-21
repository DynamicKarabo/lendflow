# cqrs-clean-architecture.md

## Intent
Encode Karabo's specific interpretation of Clean Architecture + CQRS in .NET 8. This skill
ensures an LLM stays consistent with established project conventions rather than reinventing
structure on every feature. Patterns here are opinionated — they reflect deliberate tradeoffs,
not generic recommendations.

---

## Project Structure

```
src/
├── Domain/                     # Enterprise business rules — no dependencies
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   ├── Exceptions/
│   └── Events/                 # Domain events (not integration events)
│
├── Application/                # Application business rules — depends on Domain only
│   ├── Commands/
│   │   └── CreatePayment/
│   │       ├── CreatePaymentCommand.cs
│   │       ├── CreatePaymentCommandHandler.cs
│   │       └── CreatePaymentCommandValidator.cs
│   ├── Queries/
│   │   └── GetPaymentById/
│   │       ├── GetPaymentByIdQuery.cs
│   │       ├── GetPaymentByIdQueryHandler.cs
│   │       └── PaymentDto.cs
│   ├── Common/
│   │   ├── Interfaces/         # Repository contracts, external service contracts
│   │   ├── Behaviours/         # MediatR pipeline behaviours
│   │   └── Exceptions/
│   └── EventHandlers/          # Domain event handlers
│
├── Infrastructure/             # External concerns — depends on Application
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/     # EF Core entity configurations
│   │   ├── Repositories/
│   │   └── Migrations/
│   ├── Services/               # External service implementations
│   └── Messaging/              # Azure Service Bus, etc.
│
└── Api/                        # Entry point — depends on Infrastructure + Application
    ├── Controllers/
    ├── Middleware/
    └── Program.cs
```

---

## CQRS Conventions

### Commands
- Commands mutate state — they return either `Unit` or a strongly-typed result (e.g. created resource ID)
- Naming: `VerbNounCommand` — `CreatePaymentCommand`, `CapturePaymentCommand`, `RefundPaymentCommand`
- Commands go through MediatR — controllers never call repositories directly
- Validation via FluentValidation registered as a MediatR pipeline behaviour — not in the handler

```csharp
// Command
public record CreatePaymentCommand(
    Guid MerchantId,
    decimal Amount,
    string Currency,
    string IdempotencyKey
) : IRequest<CreatePaymentResult>;

// Handler — only business logic, no validation
public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, CreatePaymentResult>
{
    public async Task<CreatePaymentResult> Handle(CreatePaymentCommand command, CancellationToken ct)
    {
        // Domain logic here
    }
}

// Validator
public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Currency).Length(3);
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
```

### Queries
- Queries never mutate state — pure reads
- Queries return DTOs, never domain entities — domain entities do not leak out of Application layer
- Naming: `GetNounByXQuery`, `ListNounsQuery`
- For read-heavy queries, bypass EF Core and use Dapper directly — do not load change tracking overhead for reads

```csharp
public record GetPaymentByIdQuery(Guid PaymentId, Guid MerchantId) : IRequest<PaymentDto?>;

// DTOs are flat — no domain logic
public record PaymentDto(
    Guid Id,
    decimal Amount,
    string Currency,
    string Status,
    DateTimeOffset CreatedAt
);
```

### MediatR Pipeline Behaviours (order matters)

```
Request → LoggingBehaviour → ValidationBehaviour → Handler → Response
```

1. `LoggingBehaviour` — logs request name, timing, outcome
2. `ValidationBehaviour` — runs FluentValidation, throws `ValidationException` before handler executes
3. Handler executes

---

## Domain Layer Rules

### Entities
- Entities protect their own invariants — no public setters on state fields
- State transitions are methods on the entity, not external mutations
- Domain events raised inside the entity, collected and dispatched after persistence

```csharp
public class Payment
{
    public PaymentStatus Status { get; private set; }
    private readonly List<IDomainEvent> _domainEvents = new();

    public void Capture()
    {
        if (Status != PaymentStatus.Authorized)
            throw new InvalidPaymentTransitionException(Status, PaymentStatus.Captured);

        Status = PaymentStatus.Captured;
        _domainEvents.Add(new PaymentCapturedEvent(Id));
    }
}
```

### Value Objects
- Any concept with no identity but meaningful equality = value object
- Examples: `Money`, `SouthAfricanIdNumber`, `WebhookEndpoint`
- Value objects are immutable — no setters, constructed via factory method with validation

### State Machines
- Complex workflows (onboarding, payment lifecycle) use Stateless library
- State machine defined in the entity or a dedicated domain service
- Invalid transitions throw domain exceptions — never silently ignored

---

## Infrastructure Layer Rules

### EF Core Conventions
- Entity configurations in separate `IEntityTypeConfiguration<T>` classes — not in `OnModelCreating`
- Global query filters for multi-tenancy — set on `DbContext`, scoped to authenticated tenant
- Audit fields (`CreatedAt`, `UpdatedAt`, `CreatedBy`) handled via `SaveChangesAsync` override
- Migrations are code-first — never edit a migration after it has been applied to any environment
- No lazy loading — always explicit `.Include()` — lazy loading hides N+1 problems

```csharp
// Multi-tenant global query filter
modelBuilder.Entity<Payment>()
    .HasQueryFilter(p => p.TenantId == _currentTenantId);
```

### Repository Pattern
- Repositories implement interfaces defined in Application layer — dependency inversion
- Generic repository only if genuinely reused — prefer specific repositories that express domain intent
- Repositories return domain entities for commands, use Dapper for query-side reads

### Idempotency Pattern
- Commands that must be idempotent accept an `IdempotencyKey`
- Check key existence before executing — Redis SET NX for atomic check-and-set
- Store result against key with TTL — return stored result on duplicate

```csharp
// Redis atomic idempotency check
var acquired = await _redis.StringSetAsync(
    key: $"idempotency:{command.IdempotencyKey}",
    value: "processing",
    expiry: TimeSpan.FromHours(24),
    when: When.NotExists
);
if (!acquired) return await GetStoredResult(command.IdempotencyKey);
```

---

## API Layer Rules

- Controllers are thin — validate auth, map to command/query, return response
- No business logic in controllers
- Consistent error responses via global exception middleware
- Problem Details (RFC 7807) for error responses

```csharp
[HttpPost]
public async Task<IActionResult> CreatePayment(
    CreatePaymentRequest request,
    CancellationToken ct)
{
    var command = new CreatePaymentCommand(
        MerchantId: CurrentMerchantId,
        Amount: request.Amount,
        Currency: request.Currency,
        IdempotencyKey: request.IdempotencyKey
    );

    var result = await _mediator.Send(command, ct);
    return CreatedAtAction(nameof(GetPayment), new { id = result.PaymentId }, result);
}
```

---

## Anti-Patterns

| Anti-Pattern | Why It's Wrong |
|---|---|
| Business logic in controllers | Untestable, violates single responsibility |
| Domain entities returned from queries | Leaks domain layer, couples read/write models |
| Public setters on entity state fields | Bypasses invariant enforcement |
| Validation inside handlers | Mixes concerns — validation is a pipeline concern |
| Lazy loading enabled | Hides N+1 — always explicit includes |
| Repositories calling other repositories | Coordination belongs in Application layer |
| `SaveChanges` called inside domain methods | Domain is persistence-ignorant |
| LLM-generated fat controllers | Common failure mode — always push logic down |

---

## Pre-Feature Checklist

- [ ] Command/Query named correctly and placed in right folder
- [ ] Validator registered — not inline in handler
- [ ] Handler contains only business logic — no HTTP concerns, no validation
- [ ] Entity state transitions are methods — no external mutation of state fields
- [ ] Domain events raised in entity, dispatched after `SaveChangesAsync`
- [ ] Query returns DTO — no domain entity leaking to API layer
- [ ] Read queries use Dapper if EF change tracking not needed
- [ ] Multi-tenant query filter will apply — not bypassed by raw SQL
- [ ] Idempotency handled if command is retry-prone
