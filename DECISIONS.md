# Architectural Decision Records (ADRs)

> This document records all architectural decisions made for the LendFlow project.

---

## ADR-001: Use .NET 9.0 as the Runtime Platform

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing the primary runtime platform for the LendFlow API.

### Decision
Use .NET 9.0 (C#) as the runtime platform.

### Rationale
- **Mature ecosystem**: .NET has a mature, well-documented ecosystem
- **Enterprise-ready**: Strong support for enterprise features (dependency injection, middleware, health checks)
- **Performance**: Excellent performance benchmarks
- **Team expertise**: Primary stack familiarity
- **LTS**: .NET 9 is the current stable release with long-term support

### Consequences
- ✅ Cross-platform support (Windows, Linux, macOS)
- ✅ Extensive NuGet package ecosystem
- ✅ Strong IDE support (Visual Studio, Rider, VS Code)
- ❌ Platform lock-in to Microsoft ecosystem

---

## ADR-002: Use ASP.NET Core Minimal APIs

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing between traditional MVC controllers and Minimal APIs for the API layer.

### Decision
Use ASP.NET Core Minimal APIs.

### Rationale
- **Performance**: Minimal APIs have lower latency than MVC controllers
- **Simplicity**: Less boilerplate code
- **Routing**: Declarative route definitions directly on handlers
- **Modern**: Aligned with modern .NET trends

### Alternatives Considered
- **MVC Controllers**: More familiar, better for complex scenarios, but adds unnecessary overhead

### Consequences
- ✅ Faster request processing
- ✅ Less code to maintain
- ✅ Cleaner route definitions
- ❌ Less familiar to developers used to MVC

---

## ADR-003: Adopt Clean Architecture + CQRS

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing the overall application architecture pattern.

### Decision
Adopt Clean Architecture with CQRS (Command Query Responsibility Segregation).

### Architecture Layers
```
┌─────────────────────────────────────┐
│         Presentation Layer          │
│     (API / Controllers)            │
├─────────────────────────────────────┤
│        Application Layer           │
│   (Commands / Queries / Handlers)   │
├─────────────────────────────────────┤
│          Domain Layer               │
│  (Entities / Value Objects / Events)│
├─────────────────────────────────────┤
│       Infrastructure Layer           │
│ (Persistence / External Services)    │
└─────────────────────────────────────┘
```

### Rationale
- **Separation of concerns**: Clear boundaries between layers
- **Testability**: Each layer can be tested independently
- **Maintainability**: Changes in one layer don't ripple to others
- **Scalability**: CQRS allows optimizing read/write paths separately

### Consequences
- ✅ Highly testable and maintainable
- ✅ Clear dependency flow (inward only)
- ✅ Flexible to evolve
- ❌ Initial setup complexity
- ❌ More files/folders to manage

---

## ADR-004: Use MediatR for CQRS Implementation

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing a library to implement the Mediator pattern for CQRS.

### Decision
Use MediatR (version 14.x) for command/query dispatch and pipeline behaviors.

### Rationale
- **Industry standard**: Widely adopted in .NET ecosystem
- **Pipeline behaviors**: Built-in support for cross-cutting concerns (validation, logging)
- **DI integration**: Seamless integration with Microsoft DI container
- **FluentValidation**: Excellent integration with FluentValidation

### Consequences
- ✅ Clean command/query handler registration
- ✅ Reusable pipeline behaviors
- ✅ Easy to add new behaviors (performance, caching)
- ❌ Additional dependency

---

## ADR-005: Use Entity Framework Core for Write Operations

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing an ORM for data persistence on the write side.

### Decision
Use Entity Framework Core 9.0 for all write operations.

### Rationale
- **Productivity**: Rapid development with migrations, change tracking
- **Type safety**: Strongly typed queries with LINQ
- **Multi-tenancy**: Built-in global query filters support
- **Relationships**: Excellent support for complex entity relationships

### Alternatives Considered
- **Dapper**: Faster but requires more boilerplate, better for read-heavy scenarios

### Consequences
- ✅ Fast development velocity
- ✅ Automatic change tracking
- ✅ Database-agnostic (can switch providers)
- ❌ Slightly slower than raw ADO.NET for bulk operations

---

## ADR-006: Use Dapper for Read Operations

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing a data access strategy for read-heavy query operations.

### Decision
Use Dapper for read operations (queries).

### Rationale
- **Performance**: Micro-ORM with minimal overhead
- **No change tracking**: Optimized for read-only queries
- **Raw SQL**: Full control over generated queries
- **Simple**: Lightweight, no configuration needed

### Rationale for CQRS
Separating read (Dapper) from write (EF Core) allows optimization of each path:
- Writes need change tracking and relationships
- Reads need speed and simplicity

### Consequences
- ✅ Fast read performance
- ✅ Full SQL control
- ✅ No EF Core overhead
- ❌ Manual query maintenance

---

## ADR-007: Use SQL Server as Primary Database

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing the primary relational database.

### Decision
Use SQL Server as the primary database.

### Rationale
- **Enterprise**: Standard for enterprise financial applications
- **ACID**: Strong transactional support for financial operations
- **Azure**: Native Azure integration (for future cloud deployment)
- **Hangfire**: Built-in support in Hangfire for job storage

### Alternatives Considered
- **PostgreSQL**: Open-source, but less enterprise integration
- **MySQL**: Less feature-rich for complex queries

### Consequences
- ✅ Excellent financial data integrity
- ✅ Strong Azure ecosystem
- ✅ Mature tooling
- ❌ Windows licensing (mitigated by containerized deployment)

---

## ADR-008: Use Redis for Caching and Idempotency

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing a caching and distributed state solution.

### Decision
Use Redis for caching and idempotency keys.

### Use Cases
1. **Idempotency Keys**: Store idempotency keys to prevent duplicate operations
2. **Distributed Locking**: Prevent race conditions in financial operations
3. **Session Cache**: Tenant/user context caching

### Rationale
- **Speed**: In-memory data store with sub-millisecond latency
- **TTL**: Built-in expiration for idempotency keys
- **SET NX**: Atomic idempotency check with single command
- **Industry standard**: Widely used in distributed systems

### Consequences
- ✅ Fast idempotency checks
- ✅ Distributed operation support
- ✅ TTL-based automatic cleanup
- ❌ Additional infrastructure requirement

---

## ADR-009: Use Stateless for State Machines

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing a library to implement explicit state transitions for entities.

### Decision
Use the Stateless library for state machine implementation.

### Entities with State Machines
- **LoanApplication**: Draft → Submitted → UnderReview → Approved/Rejected
- **Loan**: PendingDisbursement → Active → Settled/Defaulted
- **Repayment**: Scheduled → Paid

### Rationale
- **Explicit transitions**: Forces valid state changes only
- **Compiler errors**: Invalid transitions become compile-time errors
- **Audit-friendly**: Easy to log state transitions
- **Testable**: State machines are easy to unit test

### Alternatives Considered
- **Manual implementation**: Custom switch statements (error-prone)

### Consequences
- ✅ Impossible invalid state transitions
- ✅ Self-documenting code
- ✅ Easy to add new states/triggers
- ❌ Learning curve for team

---

## ADR-010: Implement Multi-Tenancy from Day One

**Date:** March 2026  
**Status:** Accepted

### Context
Deciding when to implement multi-tenancy support.

### Decision
Implement multi-tenancy from day one (not as an afterthought).

### Implementation
1. **TenantId on every entity**: All tables have TenantId foreign key
2. **Global query filters**: EF Core automatically filters by tenant
3. **JWT tenant claims**: Tenant ID extracted from JWT token
4. **Tenant middleware**: Resolves tenant from authenticated requests

### Rationale
- **Scalability**: Multiple lenders on single platform
- **Data isolation**: Enforced at database level
- **Security**: Prevents cross-tenant data leakage
- **Compliance**: Supports tenant-specific data policies

### Consequences
- ✅ Built-in tenant isolation
- ✅ No migration needed later
- ✅ Tenant-aware by default
- ❌ Slightly more complex queries

---

## ADR-011: Use JWT Bearer Authentication

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing an authentication mechanism.

### Decision
Use JWT Bearer tokens for authentication.

### Roles
- **admin**: Full system access
- **underwriter**: Can decision pending applications
- **system**: Internal service-to-service calls

### Required Claims
```json
{
  "tenant_id": "UUID",
  "role": "admin|underwriter|system",
  "iss": "lendflow",
  "aud": "lendflow-api"
}
```

### Rationale
- **Stateless**: No server-side session storage needed
- **Scalable**: Works across multiple API instances
- **Industry standard**: Widely supported, well-understood
- **Expiration**: Built-in token expiration

### Consequences
- ✅ Stateless authentication
- ✅ Works with load balancers
- ✅ Fine-grained authorization
- ❌ Token management on client side

---

## ADR-012: Use FluentValidation for Request Validation

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing a validation library for request DTOs.

### Decision
Use FluentValidation with MediatR pipeline integration.

### Rationale
- **Fluent API**: Readable, chainable validation rules
- **MediatR integration**: Automatic validation in pipeline
- **Complex rules**: Supports complex validation scenarios
- **Testable**: Easy to unit test validators

### Consequences
- ✅ Clean validation code
- ✅ Reusable validators
- ✅ Automatic error responses
- ❌ Additional dependency

---

## ADR-013: Use Problem Details (RFC 7807) for Errors

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing an error response format.

### Decision
Use Problem Details (RFC 7807) for all error responses.

### Example Response
```json
{
  "type": "https://lendflow.co.za/errors/validation-failed",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "amount": ["Amount must be between 100 and 50000"]
  }
}
```

### Rationale
- **Standard**: Industry standard (RFC 7807)
- **Consistent**: Same format across all endpoints
- **Machine-readable**: Structured for automated error handling
- **Human-readable**: Clear error messages

### Consequences
- ✅ Consistent error format
- ✅ Client-friendly errors
- ✅ Standard compliance

---

## ADR-014: Use Hangfire for Background Jobs

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing a background job processing solution.

### Decision
Use Hangfire for scheduled and background jobs.

### Jobs Implemented
| Job | Schedule | Description |
|-----|----------|-------------|
| CreditAssessmentJob | Event-triggered | Run credit scoring |
| RepaymentStatusJob | Daily 06:00 UTC | Mark late/missed repayments |
| RepaymentReminderJob | Daily 08:00 UTC | Send reminders |
| RetentionCleanupJob | Weekly Sunday 03:00 | Archive old records |

### Rationale
- **Dashboard**: Built-in dashboard for job monitoring
- **Retries**: Automatic retry logic
- **Persistence**: Jobs stored in SQL Server
- **Scheduling**: Cron expressions for complex schedules

### Alternatives Considered
- **Quartz.NET**: More complex, no dashboard
- **Azure Functions**: Vendor lock-in

### Consequences
- ✅ Easy job monitoring
- ✅ Reliable job execution
- ✅ SQL Server storage (same as main DB)
- ❌ Additional SQL Server dependency

---

## ADR-015: Use Azure Service Bus for Domain Events

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing a messaging solution for internal domain events.

### Decision
Use Azure Service Bus for publishing and consuming domain events.

### Events
| Event | Published When |
|-------|----------------|
| application.submitted | Application submitted |
| application.assessed | Credit scored |
| application.approved | Application approved |
| loan.created | Loan created |
| loan.disbursed | Funds disbursed |
| repayment.paid | Payment recorded |

### Rationale
- **Reliability**: Guaranteed delivery
- **Ordering**: Message ordering support
- **Azure integration**: Works well with Azure ecosystem
- **Decoupling**: Loosely coupled microservices

### Consequences
- ✅ Reliable event delivery
- ✅ Service decoupling
- ✅ Future microservices ready
- ❌ Currently stubbed (V1)

---

## ADR-016: Implement Idempotent Financial Operations

**Date:** March 2026  
**Status:** Accepted

### Context
Ensuring financial operations are idempotent to prevent duplicate charges/disbursements.

### Decision
Implement Redis-based idempotency for all financial operations.

### Covered Operations
- POST /applications
- POST /loans/{id}/disburse
- POST /loans/{id}/repayments/pay

### Implementation
```
1. Client sends idempotency_key with request
2. Check Redis: SET NX idempotency:{key}
3. If key exists → return cached response
4. If new → execute operation, cache response
5. TTL: 24 hours (configurable)
```

### Rationale
- **Financial integrity**: Prevents double disbursements
- **Network retry safety**: Safe to retry on timeout
- **Client simplicity**: Clients don't need complex retry logic

### Consequences
- ✅ No duplicate operations
- ✅ Safe retries
- ✅ Redis required

---

## ADR-017: Encrypt PII at Rest (POPIA Compliance)

**Date:** March 2026  
**Status:** Accepted

### Context
Complying with POPIA (Protection of Personal Information Act) for South Africa.

### Decision
Encrypt sensitive PII fields at rest using AES-256.

### Encrypted Fields
- **IdNumber**: South African ID number
- **PhoneNumber**: Contact phone number

### Implementation
- AES-256 encryption
- Keys from configuration (Azure Key Vault-ready)
- Encrypted values stored as Base64 strings

### Rationale
- **POPIA compliance**: Required for South African financial institutions
- **Data breach protection**: Even if DB is compromised, PII is unreadable
- **Key rotation**: Encryption key can be rotated without data migration

### Consequences
- ✅ POPIA compliant
- ✅ Data breach protection
- ✅ Regulatory approval
- ❌ Slight performance overhead on read

---

## ADR-018: Implement Append-Only Audit Log

**Date:** March 2026  
**Status:** Accepted

### Context
Ensuring full auditability of all system actions for FICA compliance.

### Decision
Implement append-only AuditLog table (no UPDATE/DELETE).

### AuditLog Fields
- TenantId
- EntityType
- EntityId
- Action
- PreviousState
- NewState
- PerformedBy
- OccurredAt
- Metadata (JSON)

### Rationale
- **FICA compliance**: Required for financial institutions
- **Non-repudiation**: All actions traceable to actors
- **Forensics**: Easy to reconstruct state changes
- **Legal**: Acceptable in court of law

### Consequences
- ✅ Full audit trail
- ✅ Regulatory compliance
- ✅ Forensics capability
- ❌ Large table growth (mitigated by retention policy)

---

## ADR-019: Implement Credit Scoring Engine

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing how to assess creditworthiness of applicants.

### Decision
Implement rule-based credit scoring engine (not ML/AI).

### Scoring Factors
| Factor | Weight | Description |
|--------|--------|-------------|
| Employment Status | 25% | Employed=100, SelfEmployed=75, Unemployed=25 |
| Income Stability | 25% | Based on monthly income thresholds |
| Debt-to-Income | 30% | DTI ratio scoring |
| Loan Amount | 20% | Loan vs income ratio |

### Score Mapping
| Score Range | Risk Band | Decision |
|-------------|-----------|----------|
| 651-850 | Low | Auto-Approve |
| 550-650 | Medium | Under Review |
| 300-549 | High | Auto-Reject |

### Rationale
- **Explainability**: Rule-based = explainable decisions
- **Compliance**: Easier to prove fair lending
- **Simplicity**: No training data needed
- **Control**: Full control over factors/weights

### Consequences
- ✅ Explainable decisions
- ✅ Easy to adjust rules
- ✅ No ML infrastructure
- ❌ Less accurate than ML

---

## ADR-020: Implement Decision Engine with Hard Rules

**Date:** March 2026  
**Status:** Accepted

### Context
Deciding how to make approval/rejection decisions.

### Decision
Implement two-stage decision engine:
1. **Hard Rules**: Immediate rejection if rules violated
2. **Score-Based**: Decision based on credit score

### Hard Rejection Rules
| Rule | Condition |
|------|-----------|
| Minimum Age | Age < 18 |
| Minimum Income | MonthlyIncome < R2,500 |
| Affordability | DTI > 40% |
| Existing Loan | Has active loan on platform |

### Rationale
- **NCA compliance**: Required affordability checks
- **Risk management**: Hard rules prevent obvious bad loans
- **Separation**: Clear distinction between rules and scoring

### Consequences
- ✅ NCA compliant
- ✅ Automatic high-risk rejection
- ✅ Human review for borderline cases

---

## ADR-021: Use Serilog for Structured Logging

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing a logging framework.

### Decision
Use Serilog with structured JSON logging.

### Configuration
- Console sink for development
- File sink for production
- Exclude PII from log fields

### Rationale
- **Structured**: JSON format for easy querying
- **Performance**: Low allocation logging
- **Extensible**: Many sinks available
- **POPIA**: Easy to filter out PII

### Consequences
- ✅ Queryable logs
- ✅ Easy debugging
- ✅ POPIA-compliant logging
- ❌ JSON logs harder to read in console

---

## ADR-022: Use OpenTelemetry for Observability

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing a solution for distributed tracing.

### Decision
Use OpenTelemetry for request tracing.

### Rationale
- **Standard**: Industry standard for observability
- **Vendor-neutral**: Works with many backends
- **Automatic**: Automatic span creation for HTTP requests
- **Future**: Ready for microservices observability

### Consequences
- ✅ Distributed tracing
- ✅ Performance monitoring
- ✅ Vendor flexibility

---

## ADR-023: Use Swagger/OpenAPI for API Documentation

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing how to document the API.

### Decision
Use Swagger UI with OpenAPI (Swashbuckle).

### Rationale
- **Interactive**: Browse and test API in browser
- **Standard**: Industry standard documentation
- **Auto-generated**: Generated from code attributes
- **Client generation**: Can generate client SDKs

### Consequences
- ✅ Easy API exploration
- ✅ Built-in testing
- ✅ Standard documentation

---

## ADR-024: Use xUnit for Testing

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing a testing framework.

### Decision
Use xUnit for all unit tests.

### Test Categories
- **Command Tests**: Test command handlers
- **Query Tests**: Test query handlers
- **Validator Tests**: Test FluentValidation validators
- **Domain Tests**: Test domain entities and logic

### Rationale
- **.NET standard**: Most popular .NET testing framework
- **Fact/Theory**: Flexible test organization
- **Assertions**: FluentAssertions for readable assertions

### Consequences
- ✅ Industry standard
- ✅ Good assertion library
- ✅ Test slicing support

---

## ADR-025: Stub External Integrations for V1

**Date:** March 2026  
**Status:** Accepted

### Context
How to handle external service integrations.

### Decision
Stub all external integrations for V1, implement interfaces for future.

### Stubbed Services
| Service | Interface | V1 Behavior |
|---------|-----------|-------------|
| KYC | IKycProvider | Always returns verified |
| Credit Bureau | ICreditBureauProvider | Returns empty history |
| Payment | IPaymentProcessor | Always returns success |
| Notifications | INotificationService | Logs to console |

### Rationale
- **Velocity**: Ship V1 without external dependencies
- **Testable**: Easy to test without external calls
- **Future-ready**: Interfaces ready for real implementation
- **Replaceable**: Swap stubs for real services

### Consequences
- ✅ Fast V1 delivery
- ✅ Easy testing
- ✅ Clear upgrade path
- ❌ No real integrations in V1

---

## ADR-026: Use Decimal for Financial Amounts

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing data types for financial amounts.

### Decision
Use `decimal(18,4)` for all financial amounts.

### Fields Using Decimal
- MonthlyIncome: decimal(18,4)
- MonthlyExpenses: decimal(18,4)
- RequestedAmount: decimal(18,4)
- PrincipalAmount: decimal(18,4)
- InterestRate: decimal(8,4)
- OutstandingBalance: decimal(18,4)
- AmountDue: decimal(18,4)
- AmountPaid: decimal(18,4)

### Rationale
- **Precision**: decimal avoids floating-point errors
- **Accounting**: Standard for financial calculations
- **Range**: Supports up to 999 trillion with 4 decimal places

### Alternatives Considered
- **double**: Not precise enough for financial data
- **integer (cents)**: Works but harder to work with

### Consequences
- ✅ Precise calculations
- ✅ Industry standard
- ✅ No rounding errors

---

## ADR-027: Use UUID for Entity IDs

**Date:** March 2026  
**Status:** Accepted

### Context
Choosing data types for entity identifiers.

### Decision
Use UUID (GUID) for all entity primary keys.

### Rationale
- **Distributed**: Can generate IDs without database connection
- **Security**: Harder to guess than sequential IDs
- **Unique**: Globally unique across systems
- **Standard**: Supported by all databases

### Alternatives Considered
- **Auto-increment integer**: Simpler but reveals business volume
- **Hi-Lo**: More complex, same benefits as UUID

### Consequences
- ✅ No ID collisions
- ✅ Harder to enumerate
- ✅ Database-agnostic

---

## Summary

| ADR | Decision | Status |
|-----|----------|--------|
| ADR-001 | .NET 9.0 | ✅ |
| ADR-002 | Minimal APIs | ✅ |
| ADR-003 | Clean Architecture + CQRS | ✅ |
| ADR-004 | MediatR | ✅ |
| ADR-005 | EF Core (writes) | ✅ |
| ADR-006 | Dapper (reads) | ✅ |
| ADR-007 | SQL Server | ✅ |
| ADR-008 | Redis | ✅ |
| ADR-009 | Stateless | ✅ |
| ADR-010 | Multi-tenancy | ✅ |
| ADR-011 | JWT Bearer | ✅ |
| ADR-012 | FluentValidation | ✅ |
| ADR-013 | Problem Details | ✅ |
| ADR-014 | Hangfire | ✅ |
| ADR-015 | Azure Service Bus | ✅ |
| ADR-016 | Idempotency | ✅ |
| ADR-017 | PII Encryption | ✅ |
| ADR-018 | Audit Log | ✅ |
| ADR-019 | Credit Scoring | ✅ |
| ADR-020 | Decision Engine | ✅ |
| ADR-021 | Serilog | ✅ |
| ADR-022 | OpenTelemetry | ✅ |
| ADR-023 | Swagger | ✅ |
| ADR-024 | xUnit | ✅ |
| ADR-025 | Stubs | ✅ |
| ADR-026 | Decimal | ✅ |
| ADR-027 | UUID | ✅ |

---

*Last updated: March 2026*
