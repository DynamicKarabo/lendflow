# aspnetcore-pipeline.md

## Intent
Encode ASP.NET Core pipeline conventions for .NET 8 — middleware order, DI lifetime rules,
request lifecycle, and common failure modes. LLMs get middleware order wrong, create
captive dependency bugs, and generate controllers with too much responsibility.

---

## Middleware Order

Order is not optional — wrong order causes auth bypass, broken error handling, and silent failures.

```csharp
// Program.cs — correct order
var app = builder.Build();

// 1. Exception handling — must be first to catch everything downstream
app.UseExceptionHandler("/error");
// OR custom middleware:
app.UseMiddleware<GlobalExceptionMiddleware>();

// 2. HTTPS redirection
app.UseHttpsRedirection();

// 3. Static files (if applicable) — before routing
app.UseStaticFiles();

// 4. Routing — must come before auth
app.UseRouting();

// 5. CORS — after routing, before auth
app.UseCors("DefaultPolicy");

// 6. Authentication — must come before authorization
app.UseAuthentication();

// 7. Authorization
app.UseAuthorization();

// 8. Custom middleware that needs auth context (tenant resolution, etc.)
app.UseMiddleware<TenantResolutionMiddleware>();

// 9. Endpoints
app.MapControllers();
app.MapHealthChecks("/health");
```

**Rule:** Exception handling middleware must be first. Auth middleware must precede authorization. Tenant resolution must follow authentication.

---

## DI Lifetimes

Getting this wrong causes either memory leaks (too long) or captive dependency bugs (too short captured by too long).

| Lifetime | Created | Destroyed | Use For |
|---|---|---|---|
| Singleton | App start | App stop | ConnectionMultiplexer, HttpClient factory, config |
| Scoped | Per HTTP request | End of request | DbContext, repositories, current user/tenant service |
| Transient | Every resolution | After use | Lightweight stateless services |

### Captive dependency — the most common bug

```csharp
// WRONG — Singleton captures Scoped — Scoped lives forever, tenant context is wrong
services.AddSingleton<IPaymentService, PaymentService>(); // depends on IAppDbContext (Scoped)

// RIGHT — match lifetime to dependencies
services.AddScoped<IPaymentService, PaymentService>();
```

**Rule:** A service cannot depend on a shorter-lived service.
- Singleton → can only depend on Singleton
- Scoped → can depend on Singleton or Scoped
- Transient → can depend on anything

### Resolving scoped services from singletons
```csharp
// When a singleton genuinely needs a scoped service (e.g. background service)
public class SettlementBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services; // Inject provider, not scoped service

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<ISettlementRepository>();
        await repo.ProcessAsync(ct);
    }
}
```

---

## Global Exception Middleware

Never let unhandled exceptions return stack traces to clients. Never use different error shapes per controller.

```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteProblemDetailsAsync(context, "Validation failed", ex.Errors);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await WriteProblemDetailsAsync(context, ex.Message);
        }
        catch (ConflictException ex)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await WriteProblemDetailsAsync(context, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteProblemDetailsAsync(context, "An unexpected error occurred");
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext context, string title, object? errors = null)
    {
        context.Response.ContentType = "application/problem+json";
        var problem = new { title, status = context.Response.StatusCode, errors };
        await context.Response.WriteAsJsonAsync(problem);
    }
}
```

**Rules:**
- Problem Details format (RFC 7807) for all error responses — consistent shape
- Never return stack traces, exception messages, or internal details to clients
- Log at `Error` level with full exception — never swallow

---

## Tenant Resolution Middleware

```csharp
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public async Task InvokeAsync(HttpContext context, ICurrentTenantService tenantService)
    {
        // Resolve tenant from API key or JWT claim
        var tenantId = context.User.FindFirst("tenant_id")?.Value
            ?? context.Request.Headers["X-Merchant-Id"].FirstOrDefault();

        if (tenantId is null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        tenantService.SetTenant(Guid.Parse(tenantId));
        await _next(context);
    }
}
```

---

## Controller Conventions

Controllers are thin. They authenticate, map requests to commands/queries, return responses. Nothing else.

```csharp
[ApiController]
[Route("api/v1/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreatePaymentResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentRequest request,
        CancellationToken ct)
    {
        var command = new CreatePaymentCommand(
            MerchantId: User.GetMerchantId(),   // Extension method on ClaimsPrincipal
            Amount: request.Amount,
            Currency: request.Currency,
            IdempotencyKey: request.IdempotencyKey
        );

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetPayment), new { id = result.PaymentId }, result);
    }
}
```

**Rules:**
- `[ApiController]` always — automatic model validation, binding source inference
- `[ProducesResponseType]` on every endpoint — self-documenting, Swagger accurate
- Extract tenant/merchant ID from claims — never from request body
- `CancellationToken` on every async action — request cancellation propagates correctly

---

## Request Validation

Validation via FluentValidation in MediatR pipeline — not in controllers, not in handlers.

```csharp
// Registered automatically via MediatR pipeline behaviour
// Controller never touches validation — it throws ValidationException
// GlobalExceptionMiddleware catches ValidationException → 400

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be positive")
            .LessThanOrEqualTo(1_000_000).WithMessage("Amount exceeds maximum");

        RuleFor(x => x.Currency)
            .Length(3).WithMessage("Currency must be ISO 4217 3-letter code")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be uppercase");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(255);
    }
}
```

---

## Health Checks

```csharp
services.AddHealthChecks()
    .AddSqlServer(connectionString, name: "sql-server")
    .AddRedis(redisConnectionString, name: "redis")
    .AddAzureServiceBusTopic(serviceBusConnection, topicName, name: "service-bus");

// Map with detail in internal networks, minimal externally
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false  // Liveness — just "am I running"
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

---

## HttpClient Configuration

Never `new HttpClient()`. Always use `IHttpClientFactory`.

```csharp
// Named client with Polly resilience
services.AddHttpClient("webhook-dispatcher", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.AddTransientHttpErrorPolicy(policy =>
    policy.WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))))
.AddTransientHttpErrorPolicy(policy =>
    policy.CircuitBreakerAsync(5, TimeSpan.FromMinutes(1)));
```

---

## Anti-Patterns

| Anti-Pattern | Why It's Wrong |
|---|---|
| Business logic in controllers | Untestable, breaks separation of concerns |
| Exception handling middleware not first | Exceptions from other middleware uncaught |
| Authentication after authorization | Auth context missing for authorization checks |
| Scoped service injected into Singleton | Captive dependency — scoped service never disposed |
| `new HttpClient()` | Socket exhaustion, no resilience policy |
| Stack traces in error responses | Information leakage — exposes internals |
| No `CancellationToken` on actions | Client disconnect not propagated — wasted work continues |
| Validation in controllers | Duplicated, inconsistent — belongs in pipeline |
| Hardcoded tenant ID in controllers | Multi-tenant correctness bug |

---

## Checklist

- [ ] Exception middleware is first in pipeline
- [ ] Authentication before authorization in pipeline
- [ ] Tenant resolution after authentication
- [ ] DI lifetimes match — no scoped captured by singleton
- [ ] Background services create scope for scoped dependencies
- [ ] All error responses use Problem Details format
- [ ] No stack traces or internal details in error responses
- [ ] Controllers are thin — no business logic
- [ ] `CancellationToken` on every async controller action
- [ ] Validation in FluentValidation pipeline — not in controllers
- [ ] `IHttpClientFactory` used — never `new HttpClient()`
- [ ] Health checks registered and mapped
