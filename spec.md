# LendFlow — Micro-Lending Credit Application API
## Technical Specification v1.0

---

## 1. Overview

LendFlow is a multi-tenant backend API that manages the full lifecycle of a personal
micro-loan. It supports loan application intake, automated credit assessment, decisioning,
disbursement, repayment tracking, and lifecycle state management.

The system is designed for correctness and auditability first — every state transition is
explicit, every financial operation is idempotent, and every change is audited.

---

## 2. Architecture Decisions

| Concern | Decision | Rationale |
|---|---|---|
| Language | C# / .NET 8 | Primary stack |
| Framework | ASP.NET Core (Minimal APIs) | Performance, native .NET 8 |
| Architecture | Clean Architecture + CQRS | Separation of concerns, testability |
| Mediator | MediatR | Command/query dispatch, pipeline behaviours |
| ORM | EF Core 8 | Write side — with global query filters for tenancy |
| Read queries | Dapper | Query side — no change tracking overhead |
| Database | SQL Server | Primary data store |
| Cache / Idempotency | Redis | Idempotency keys, distributed locks |
| Background jobs | Hangfire | Recurring jobs, retry logic |
| Messaging | Azure Service Bus | Internal domain events across bounded contexts |
| Auth | JWT (Bearer) | Stateless, role-based |
| Error format | Problem Details (RFC 7807) | Consistent error shape across all endpoints |
| Multi-tenancy | Yes — from day one | Global query filters, TenantId on all entities |
| PII handling | POPIA-compliant | Encryption, minimisation, retention policy |

---

## 3. Goals

- Reliable API for full loan lifecycle management
- Automated credit scoring and rule-based decisioning
- Repayment tracking and outstanding balance management
- Auditability and regulatory compliance (FICA, POPIA, NCA)
- Idempotent financial operations — no double disbursements, no double charges
- Multi-tenant from day one — multiple lenders on one platform

---

## 4. Non-Goals (V1)

- No frontend UI
- No direct payment gateway UI (integration via stub interfaces only)
- No ML credit scoring (rule-based only)
- No collections workflow
- No loan restructuring
- No multi-currency (ZAR only)

---

## 5. Core Entities

### 5.1 Tenant
Represents a lender on the platform.

| Field | Type | Notes |
|---|---|---|
| Id | UUID | PK |
| Name | nvarchar(255) | Lender name |
| ApiKey | nvarchar(255) | Hashed — used for tenant resolution |
| IsActive | bit | Soft disable without deletion |
| CreatedAt | datetimeoffset | UTC |

---

### 5.2 Applicant
Represents an individual applying for credit.

| Field | Type | Notes |
|---|---|---|
| Id | UUID | PK |
| TenantId | UUID | FK — multi-tenant |
| FirstName | nvarchar(100) | |
| LastName | nvarchar(100) | |
| IdNumber | nvarchar(255) | **AES-256 encrypted — POPIA** |
| PhoneNumber | nvarchar(20) | **AES-256 encrypted — POPIA** |
| Email | nvarchar(255) | |
| DateOfBirth | date | Cross-validated against IdNumber |
| EmploymentStatus | nvarchar(50) | Employed / SelfEmployed / Unemployed |
| MonthlyIncome | decimal(18,4) | NCA affordability input |
| MonthlyExpenses | decimal(18,4) | NCA affordability input |
| CreatedAt | datetimeoffset | UTC |
| UpdatedAt | datetimeoffset | UTC |

**Rules:**
- SA ID number validated on creation: Luhn algorithm, date component, gender digit,
  citizenship digit, cross-validation against DateOfBirth
- IdNumber and PhoneNumber encrypted at rest via Azure Key Vault — never stored plaintext
- Applicant must be 18+ — derived from IdNumber, enforced at domain layer

---

### 5.3 LoanApplication
Represents a submitted request for credit.

| Field | Type | Notes |
|---|---|---|
| Id | UUID | PK |
| TenantId | UUID | FK |
| ApplicantId | UUID | FK |
| RequestedAmount | decimal(18,4) | |
| RequestedTermMonths | int | |
| Purpose | nvarchar(100) | WorkingCapital / Education / Medical / Other |
| Status | nvarchar(50) | Enum stored as string |
| CreditScore | int | Null until assessed |
| RiskBand | nvarchar(20) | Low / Medium / High — null until assessed |
| DecisionReason | nvarchar(500) | Populated on approval or rejection |
| CreatedAt | datetimeoffset | UTC |
| UpdatedAt | datetimeoffset | UTC |

**Statuses (state machine — explicit transitions only):**
```
Draft → Submitted → UnderReview → Approved
                              └→ Rejected
Submitted → Cancelled (applicant-initiated only, before UnderReview)
```

---

### 5.4 Loan
Created automatically when application is approved.

| Field | Type | Notes |
|---|---|---|
| Id | UUID | PK |
| TenantId | UUID | FK |
| ApplicationId | UUID | FK — one-to-one |
| ApplicantId | UUID | FK |
| PrincipalAmount | decimal(18,4) | May differ from RequestedAmount |
| InterestRate | decimal(8,4) | Annual rate e.g. 0.28 = 28% |
| TermMonths | int | |
| RepaymentFrequency | nvarchar(20) | Monthly (V1 only) |
| DisbursementDate | datetimeoffset | Null until disbursed |
| MaturityDate | date | Calculated on creation |
| OutstandingBalance | decimal(18,4) | Updated on each repayment |
| Status | nvarchar(50) | Enum stored as string |
| CreatedAt | datetimeoffset | UTC |

**Statuses:**
```
PendingDisbursement → Active → Settled
                   └→ Defaulted
                   └→ WrittenOff (future)
```

---

### 5.5 Repayment
Tracks individual scheduled repayments against a loan.

| Field | Type | Notes |
|---|---|---|
| Id | UUID | PK |
| TenantId | UUID | FK |
| LoanId | UUID | FK |
| InstallmentNumber | int | 1-based sequence |
| AmountDue | decimal(18,4) | |
| AmountPaid | decimal(18,4) | Null until paid |
| DueDate | date | |
| PaidDate | datetimeoffset | Null until paid |
| Status | nvarchar(20) | Scheduled / Paid / Late / Missed |
| PaymentReference | nvarchar(255) | External payment ref |
| CreatedAt | datetimeoffset | UTC |

---

### 5.6 CreditAssessment
Stores the output of a credit assessment run.

| Field | Type | Notes |
|---|---|---|
| Id | UUID | PK |
| TenantId | UUID | FK |
| ApplicationId | UUID | FK |
| Score | int | 300–850 |
| RiskBand | nvarchar(20) | Low / Medium / High |
| FactorBreakdown | nvarchar(max) | JSON — individual factor scores |
| AssessedAt | datetimeoffset | UTC |

---

### 5.7 AuditLog
Append-only audit trail — no UPDATE/DELETE grants on this table.

| Field | Type | Notes |
|---|---|---|
| Id | UUID | PK |
| TenantId | UUID | FK |
| EntityType | nvarchar(100) | LoanApplication / Loan / Repayment |
| EntityId | UUID | |
| Action | nvarchar(100) | StateTransition / Created / Updated |
| PreviousState | nvarchar(100) | Null on creation |
| NewState | nvarchar(100) | |
| PerformedBy | nvarchar(255) | UserId or system |
| OccurredAt | datetimeoffset | UTC |
| Metadata | nvarchar(max) | JSON — additional context |

---

## 6. Loan Lifecycle

```
1.  Applicant registered (POST /applicants)
2.  Application submitted (POST /applications)
        → application.submitted event published
3.  Credit assessment triggered automatically (Hangfire job)
        → Score calculated (300–850)
        → Risk band assigned (Low / Medium / High)
        → application.assessed event published
4.  Decision engine runs automatically after assessment
        → Score > 650: Approved
        → Score 550–650: UnderReview (underwriter assigned)
        → Score < 550 or rule violation: Rejected
        → application.approved / application.rejected event published
5.  On approval: Loan created automatically
        → Repayment schedule generated
        → loan.created event published
6.  Disbursement initiated (POST /loans/{id}/disburse)
        → Idempotency enforced via Redis SET NX
        → loan.disbursed event published
7.  Repayments tracked (POST /loans/{id}/repayments/pay)
        → Outstanding balance updated
        → repayment.paid event published
8.  Daily Hangfire job checks for late/missed repayments
9.  Loan settled when outstanding balance = 0
        → loan.closed event published
```

---

## 7. Credit Score Calculation (V1 Rule-Based)

**Score range: 300–850**

| Factor | Weight | Notes |
|---|---|---|
| Employment status | 25% | Employed scores highest |
| Income stability (monthly income) | 25% | Higher income = higher score |
| Debt-to-income ratio | 30% | Monthly repayment / monthly income |
| Repayment history | 20% | Previous loans on platform |

Each factor implements `ICreditScoringFactor` — independently audited, pluggable.

**Risk bands:**
- Low: 651–850
- Medium: 550–650 (triggers UnderReview)
- High: 300–549 (auto-reject)

---

## 8. Decision Engine (V1)

Hard rejection rules (applied before score):
- Applicant age < 18 (NCA requirement)
- Monthly income below minimum threshold (R2,500)
- Debt-to-income ratio > 40% (NCA affordability — proposed repayment vs income)
- Existing active loan on platform (max 1 concurrent)

Score-based rules:
- Score > 650 → Approved (auto)
- Score 550–650 → UnderReview (underwriter must decision manually)
- Score < 550 → Rejected (auto)

---

## 9. Repayment Schedule Generation

**V1: Simple interest, equal monthly installments**

```
Monthly payment = Principal × (r(1+r)^n) / ((1+r)^n - 1)
where r = monthly interest rate (annual rate / 12)
      n = term in months

Example:
  Principal: R1,000
  Annual rate: 28%
  Monthly rate: 0.28 / 12 = 0.02333
  Term: 3 months
  Monthly payment: R348.54
```

Schedule generated on loan creation. One `Repayment` row per instalment.

---

## 10. API Design

**Base URL:** `/api/v1`

**Authentication:** JWT Bearer token

**Roles:**
- `admin` — full access
- `underwriter` — can decision UnderReview applications
- `system` — internal service-to-service

**Idempotency:** All state-mutating POST endpoints require `Idempotency-Key` header.

**Error format:** Problem Details (RFC 7807)
```json
{
  "type": "https://lendflow.co.za/errors/validation-failed",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "errors": {
    "amount": ["Amount must be between R100 and R50,000"]
  }
}
```

---

## 11. Endpoints

### Applicants

| Method | Path | Description | Auth |
|---|---|---|---|
| POST | `/applicants` | Register applicant | admin |
| GET | `/applicants/{id}` | Get applicant by ID | admin, underwriter |

---

### Loan Applications

| Method | Path | Description | Auth |
|---|---|---|---|
| POST | `/applications` | Submit loan application | admin |
| GET | `/applications/{id}` | Get application by ID | admin, underwriter |
| GET | `/applications` | List applications (filterable by status) | admin, underwriter |
| POST | `/applications/{id}/cancel` | Cancel application (Submitted state only) | admin |
| POST | `/applications/{id}/decision` | Manual decision (UnderReview only) | underwriter |

**POST /applications body:**
```json
{
  "applicant_id": "uuid",
  "amount": 1000.00,
  "term_months": 3,
  "purpose": "working_capital",
  "idempotency_key": "uuid"
}
```

**POST /applications/{id}/decision body:**
```json
{
  "decision": "approved",
  "reason": "Manual review passed"
}
```

---

### Loans

| Method | Path | Description | Auth |
|---|---|---|---|
| GET | `/loans/{id}` | Get loan by ID | admin, underwriter |
| GET | `/loans` | List loans (filterable by status) | admin |
| POST | `/loans/{id}/disburse` | Initiate disbursement | admin |

**POST /loans/{id}/disburse body:**
```json
{
  "method": "bank_transfer",
  "account_number": "...",
  "bank_code": "...",
  "idempotency_key": "uuid"
}
```

---

### Repayments

| Method | Path | Description | Auth |
|---|---|---|---|
| GET | `/loans/{id}/repayments` | Get repayment schedule | admin, underwriter |
| POST | `/loans/{id}/repayments/pay` | Record repayment | admin |

**POST /loans/{id}/repayments/pay body:**
```json
{
  "amount": 348.54,
  "payment_reference": "EFT-ABC123",
  "idempotency_key": "uuid"
}
```

---

## 12. Domain Events (Internal — Azure Service Bus)

| Event | Published When | Consumers |
|---|---|---|
| `application.submitted` | Application moves to Submitted | CreditAssessmentJob (Hangfire trigger) |
| `application.assessed` | Credit score calculated | DecisionEngine |
| `application.approved` | Application approved | LoanCreationHandler |
| `application.rejected` | Application rejected | NotificationService |
| `loan.created` | Loan record created | RepaymentScheduleGenerator |
| `loan.disbursed` | Disbursement confirmed | NotificationService |
| `repayment.paid` | Repayment recorded | BalanceUpdateHandler, NotificationService |
| `repayment.due` | 3 days before due date | NotificationService (reminder) |
| `loan.closed` | Outstanding balance = 0 | AuditService |

---

## 13. Background Jobs (Hangfire)

| Job | Schedule | Description |
|---|---|---|
| `CreditAssessmentJob` | Triggered by event | Run credit scoring on submitted application |
| `RepaymentStatusJob` | Daily 06:00 UTC | Mark late/missed repayments, check outstanding |
| `RepaymentReminderJob` | Daily 08:00 UTC | Publish `repayment.due` for upcoming instalments |
| `RetentionCleanupJob` | Weekly Sunday 03:00 UTC | Archive records past retention window |

---

## 14. External Integrations (Stubbed in V1)

All external integrations are behind interfaces — stubbed for V1, real implementations in V2.

| Integration | Interface | Stub behaviour |
|---|---|---|
| KYC provider | `IKycProvider` | Always returns verified |
| Credit bureau | `ICreditBureauProvider` | Returns empty history |
| Payment processor | `IPaymentProcessor` | Always returns success |
| SMS / Email | `INotificationService` | Logs to console |

---

## 15. Compliance

### POPIA
- `IdNumber` and `PhoneNumber` encrypted at rest (AES-256, Azure Key Vault)
- No PII in logs or error responses
- No PII in event payloads — reference IDs only, resolve on consumption
- 5-year retention policy enforced by `RetentionCleanupJob`

### FICA
- SA ID number validated on applicant creation (Luhn, date, gender digit, citizenship digit, DOB cross-validation)
- Audit log is append-only — no UPDATE/DELETE grants at DB level
- Every state transition recorded with actor, timestamp, reason

### NCA
- Affordability assessment enforced before approval (debt-to-income check)
- Minimum age 18 enforced at domain layer
- Maximum debt-to-income ratio 40%

---

## 16. Idempotency

Required on all state-mutating endpoints. Enforced via Redis SET NX.

| Endpoint | Idempotency Key Source |
|---|---|
| POST /applications | Request body `idempotency_key` |
| POST /loans/{id}/disburse | Request body `idempotency_key` |
| POST /loans/{id}/repayments/pay | Request body `idempotency_key` |
| POST /applications/{id}/cancel | Generated from `applicationId + "cancel"` |

---

## 17. Observability

- Structured logging via Serilog — no PII in log fields
- Request tracing via OpenTelemetry
- Hangfire dashboard — authenticated, admin role only
- Health checks: SQL Server, Redis, Azure Service Bus

---

## 18. Database Index Strategy

Critical indexes (beyond PKs):

| Table | Index | Reason |
|---|---|---|
| LoanApplications | `(TenantId, Status)` | Primary filter for list queries |
| LoanApplications | `(TenantId, ApplicantId)` | Lookup by applicant |
| Loans | `(TenantId, Status)` | List active loans |
| Loans | `(ApplicationId)` | FK — not auto-indexed |
| Repayments | `(LoanId, Status)` | Repayment schedule lookup |
| Repayments | `(DueDate, Status)` | Daily job filter — late/missed detection |
| AuditLog | `(EntityId, EntityType)` | Audit trail lookup by entity |

---

## 19. Skills Reference

This project is built following conventions from `karabo-skills`:

| Skill | Applied To |
|---|---|
| `dotnet/cqrs-clean-architecture.md` | All features |
| `dotnet/ef-core-patterns.md` | All DB work |
| `dotnet/aspnetcore-pipeline.md` | Middleware, controllers, DI |
| `infrastructure/redis-patterns.md` | Idempotency, distributed locks |
| `infrastructure/azure-service-bus-patterns.md` | Domain events |
| `infrastructure/hangfire-patterns.md` | Background jobs |
| `data/sql-server-performance.md` | Index design, query optimisation |
| `compliance/sa-compliance-patterns.md` | Applicant, KYC, audit trail |

---

## 20. Build Phases

### Phase 1 — Foundation
Solution structure, domain entities, state machines, EF Core setup, first command + query.

### Phase 2 — Compliance Layer
SA ID validation, affordability check, audit trail, PII encryption.

### Phase 3 — Credit Assessment + Decisioning
Scoring engine, decision rules, Hangfire job, domain events.

### Phase 4 — Loan + Repayment
Loan creation, repayment schedule generation, repayment capture, balance tracking.

### Phase 5 — Background Jobs + Observability
Daily status jobs, reminder jobs, retention cleanup, structured logging, health checks.
