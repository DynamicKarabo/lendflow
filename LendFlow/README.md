# LendFlow - Micro-Lending Credit Application API

![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![License](https://img.shields.io/badge/license-MIT-blue)
![Status](https://img.shields.io/badge/status-Production%20Ready-green)

**LendFlow** is an enterprise-grade, multi-tenant backend API for managing the full lifecycle of personal micro-loans. Built with Clean Architecture + CQRS pattern for South African lending institutions.

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              LendFlow Architecture                           │
└─────────────────────────────────────────────────────────────────────────────┘

                                    ┌─────────────┐
                                    │   Clients   │
                                    └──────┬──────┘
                                           │
                                    ┌──────▼──────┐
                                    │   API Layer │
                                    │  (Minimal)  │
                                    └──────┬──────┘
                                           │
                    ┌──────────────────────┼──────────────────────┐
                    │                      │                      │
             ┌──────▼──────┐      ┌──────▼──────┐      ┌──────▼──────┐
             │  Applicants  │      │ Applications │      │    Loans     │
             │  Controller  │      │  Controller  │      │  Controller  │
             └──────┬──────┘      └──────┬──────┘      └──────┬──────┘
                    │                      │                      │
                    └──────────────────────┼──────────────────────┘
                                           │
                    ┌──────────────────────▼──────────────────────┐
                    │              Application Layer (CQRS)           │
                    │  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
                    │  │ Commands  │  │  Queries │  │  Credit  │  │
                    │  │ Handlers  │  │  Handlers│  │ Scoring  │  │
                    │  └──────────┘  └──────────┘  └──────────┘  │
                    └──────────────────────┬──────────────────────┘
                                           │
                    ┌──────────────────────▼──────────────────────┐
                    │                 Domain Layer                  │
                    │  ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐ ┌─────┐  │
                    │  │Tenant│ │Loan │ │ Rep │ │Credit│ │Audit│  │
                    │  │Entity│ │State│ │aymnt│ │Assess│ │ Log │  │
                    │  └─────┘ └─────┘ └─────┘ └─────┘ └─────┘  │
                    └──────────────────────┬──────────────────────┘
                                           │
                    ┌──────────────────────▼──────────────────────┐
                    │             Infrastructure Layer                │
                    │  ┌────────┐ ┌────────┐ ┌─────────────────┐   │
                    │  │  EF    │ │ Redis  │ │  Idempotency   │   │
                    │  │ Core 8 │ │  Cache │ │    Service     │   │
                    │  └────────┘ └────────┘ └─────────────────┘   │
                    └───────────────────────────────────────────────┘
```

---

## Entity Relationship Diagram

```
┌─────────────────┐         ┌──────────────────────┐         ┌─────────────────┐
│     Tenant      │         │     LoanApplication    │         │    Applicant     │
├─────────────────┤         ├──────────────────────┤         ├─────────────────┤
│ Id (PK)         │         │ Id (PK)               │         │ Id (PK)          │
│ Name            │         │ TenantId (FK)         │         │ TenantId (FK)    │
│ ApiKeyHash      │         │ ApplicantId (FK) ─────┼────────►│ FirstName        │
│ IsActive        │         │ RequestedAmount       │         │ LastName         │
│ CreatedAt       │         │ RequestedTermMonths   │         │ IdNumber (enc)   │
└─────────────────┘         │ Purpose               │         │ PhoneNumber(enc) │
        │                    │ Status [State Machine]│         │ Email           │
        │                    │ CreditScore           │         │ DateOfBirth     │
        │                    │ RiskBand              │         │ EmploymentStatus │
        │                    │ DecisionReason        │         │ MonthlyIncome    │
        │                    └──────────┬───────────┘         │ MonthlyExpenses  │
        │                             │                      └─────────────────┘
        │                             │                              │
        │                    ┌────────▼───────────┐                      │
        │                    │  CreditAssessment   │                      │
        │                    ├────────────────────┤                      │
        │                    │ Id (PK)            │                      │
        │                    │ ApplicationId (FK) │                      │
        │                    │ Score (300-850)    │                      │
        │                    │ RiskBand           │                      │
        │                    │ FactorBreakdown    │                      │
        │                    │ AssessedAt         │                      │
        │                    └────────────────────┘                      │
        │                             │                                    │
        │                    ┌────────▼───────────┐                      │
        │                    │       Loan         │                      │
        │                    ├────────────────────┤                      │
        └───────────────────►│ Id (PK)           │                      │
                            │ ApplicationId (FK) │◄──────────────────────┘
                            │ PrincipalAmount    │
                            │ InterestRate      │
                            │ TermMonths        │
                            │ Status [State]    │
                            │ OutstandingBalance │
                            └────────┬───────────┘
                                     │
                            ┌────────▼───────────┐
                            │    Repayment       │
                            ├────────────────────┤
                            │ Id (PK)            │
                            │ LoanId (FK) ───────┘
                            │ InstallmentNumber  │
                            │ AmountDue          │
                            │ AmountPaid         │
                            │ DueDate            │
                            │ Status             │
                            │ PaymentReference    │
                            └────────────────────┘

┌─────────────────┐
│    AuditLog     │  (Append-only)
├─────────────────┤
│ Id (PK)         │
│ TenantId (FK)   │
│ EntityType      │
│ EntityId        │
│ Action          │
│ PreviousState    │
│ NewState        │
│ PerformedBy     │
│ OccurredAt      │
│ Metadata (JSON) │
└─────────────────┘
```

---

## Loan Lifecycle State Machine

```
┌──────────────────────────────────────────────────────────────────────────────┐
│                         Loan Application States                                │
└──────────────────────────────────────────────────────────────────────────────┘

    ┌─────────┐
    │  Draft  │  (Application created)
    └────┬────┘
         │ Submit
         ▼
    ┌────────────┐
───►│ Submitted  │◄──────────────────────────────────┐
    └─────┬──────┘                                   │
          │                                          │ Cancel
          │ Credit Assessment                        │ (by applicant)
          ▼                                          │
    ┌─────────────┐                                  │
    │UnderReview  │ (Score 550-650, needs human)    │
    └──────┬──────┘                                  │
           │                                         │
     ┌─────┴─────┐                                   │
     │           │                                   │
     ▼           ▼                                   │
┌─────────┐ ┌─────────┐                             │
│ Approved │ │ Rejected│                             │
└────┬────┘ └─────────┘                             │
     │                                                   │
     │ (Loan automatically created)                      │
     ▼                                                   │
┌─────────────────────────┐                           │
│  PendingDisbursement    │                           │
└───────────┬─────────────┘                           │
            │ Disburse                               │
            ▼                                         │
┌─────────────────────────┐                           │
│        Active           │◄──────────────────────────┘
└───────────┬─────────────┘  (re-borrow)
            │
     ┌──────┴──────┐
     │             │
     ▼             ▼
┌─────────┐ ┌───────────┐
│ Settled │ │ Defaulted │
└─────────┘ └───────────┘
     │             │
     │             ▼
     │     ┌───────────┐
     │     │WrittenOff │
     │     └───────────┘
     ▼
┌─────────┐
│  Closed │
└─────────┘
```

---

## Credit Scoring Algorithm

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      Credit Score Calculation (300-850)                      │
└─────────────────────────────────────────────────────────────────────────────┘

                    ┌─────────────────────────────────────────────┐
                    │           Credit Assessment Service          │
                    └──────────────────────┬──────────────────────┘
                                           │
                    ┌──────────────────────▼──────────────────────┐
                    │            Scoring Factors                   │
                    └──────────────────────┬──────────────────────┘
                                           │
        ┌─────────────────────────────────┼─────────────────────────────────┐
        │                                 │                                 │
        ▼                                 ▼                                 ▼
┌───────────────────┐           ┌───────────────────┐           ┌───────────────────┐
│ Employment Status  │           │  Income Stability  │           │  Debt-to-Income   │
│    (25% weight)   │           │   (25% weight)    │           │   (30% weight)    │
├───────────────────┤           ├───────────────────┤           ├───────────────────┤
│ Employed    → 100│           │ ≥R50,000  → 100   │           │ DTI ≤10%  → 100  │
│ SelfEmp    →  75 │           │ ≥R35,000  →  85   │           │ DTI ≤20%  →  85  │
│ Unemployed →  25 │           │ ≥R25,000  →  70   │           │ DTI ≤30%  →  70  │
└───────────────────┘           └───────────────────┘           │ DTI ≤40%  →  50  │
                                                                 │ DTI >40%  →   0  │
                                                                 └───────────────────┘
                                            │
                                            ▼
                              ┌───────────────────┐
                              │   Loan Amount     │
                              │  (20% weight)     │
                              ├───────────────────┤
                              │ ≤0.5x income → 100│
                              │ ≤1.0x income →  85│
                              │ ≤2.0x income →  70│
                              │ ≤3.0x income →  50│
                              │ >3.0x income →  25│
                              └───────────────────┘

                                            │
                    ┌───────────────────────┼───────────────────────┐
                    │         Weighted Score Aggregation              │
                    └───────────────────────┼───────────────────────┘
                                            │
                                            ▼
                    ┌───────────────────────────────────────────────┐
                    │           Normalized Score (0-100)              │
                    │                     │                           │
                    │        Score = (Factor1×25 + Factor2×25 +      │
                    │             Factor3×30 + Factor4×20) / 100     │
                    └───────────────────────┬───────────────────────┘
                                            │
                                            ▼
                    ┌───────────────────────────────────────────────┐
                    │              Mapped to 300-850 Scale            │
                    │                                               │
                    │    ┌──────────┐                               │
                    │    │   Score  │                               │
                    │    │ ≥90 → 800-850                           │
                    │    │ ≥80 → 750-799                            │
                    │    │ ≥70 → 700-749                           │
                    │    │ ≥60 → 650-699                            │
                    │    │ ≥50 → 600-649                            │
                    │    │ ≥40 → 550-599                            │
                    │    │ ≥30 → 500-549                            │
                    │    │ ≥20 → 450-499                            │
                    │    │ <20 → 400-449                            │
                    │    └──────────┘                               │
                    └───────────────────────┬───────────────────────┘
                                            │
                    ┌───────────────────────┼───────────────────────┐
                    │                  Decision                      │
                    └───────────────────────┼───────────────────────┘
                                            │
                    ┌───────────────────────┼───────────────────────┐
                    │   Score > 650         │    Score 550-650       │   Score < 550
                    │         │             │          │             │        │
                    ▼         ▼             ▼          ▼             ▼        ▼
              ┌───────────┐      ┌────────────┐      ┌──────────┐
              │ APPROVED  │      │UNDER REVIEW│      │ REJECTED │
              │  (Auto)  │      │  (Human)  │      │  (Auto) │
              └───────────┘      └────────────┘      └──────────┘
```

---

## API Endpoints

### Applicants

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/v1/applicants` | Register new applicant | admin |
| `GET` | `/api/v1/applicants/{id}` | Get applicant details | admin, underwriter |

### Loan Applications

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/v1/applications` | Submit loan application | admin |
| `GET` | `/api/v1/applications` | List applications | admin, underwriter |
| `GET` | `/api/v1/applications/{id}` | Get application details | admin, underwriter |
| `POST` | `/api/v1/applications/{id}/assess` | Trigger credit assessment | admin, system |
| `POST` | `/api/v1/applications/{id}/decision` | Manual underwriter decision | underwriter, admin |

### Loans

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/v1/loans` | List loans | admin |
| `GET` | `/api/v1/loans/{id}` | Get loan details | admin, underwriter |
| `POST` | `/api/v1/loans/{id}/disburse` | Disburse loan | admin |
| `GET` | `/api/v1/loans/{id}/repayments` | Get repayment schedule | admin, underwriter |
| `POST` | `/api/v1/loans/{id}/repayments/pay` | Record repayment | admin |

### Health Checks

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/health` | Full health check (SQL + Redis) |
| `GET` | `/health/ready` | Readiness probe |
| `GET` | `/health/live` | Liveness probe |

---

## Repayment Schedule Generation

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    Monthly Installment Calculation                           │
└─────────────────────────────────────────────────────────────────────────────┘

                         Principal: R15,000
                         Annual Rate: 28%
                         Term: 12 months

                    ┌────────────────────────┐
                    │  Monthly Rate (r)      │
                    │  r = 0.28 / 12        │
                    │  r = 0.02333          │
                    └───────────┬────────────┘
                                │
                                ▼
         ┌─────────────────────────────────────────────────┐
         │  EMI Formula:                                    │
         │                                                 │
         │  EMI = P × [r(1+r)^n] / [(1+r)^n - 1]         │
         │                                                 │
         │  EMI = 15000 × [0.02333 × (1.02333)^12]       │
         │          ÷ [(1.02333)^12 - 1]                 │
         │                                                 │
         │  EMI = R1,447.59/month                         │
         └─────────────────────┬───────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Repayment Schedule                                   │
└─────────────────────────────────────────────────────────────────────────────┘

Month │  Due Date   │ Amount Due │ Amount Paid │  Balance After  │
──────┼─────────────┼────────────┼─────────────┼────────────────│
  1   │ 2026-04-23 │  R1,447.59 │      -      │  R16,923.49   │
  2   │ 2026-05-23 │  R1,447.59 │      -      │  R15,923.49   │
  3   │ 2026-06-23 │  R1,447.59 │  R1,447.59  │  R14,475.90   │
 ...  │    ...      │     ...    │     ...     │     ...       │
 12   │ 2027-03-23 │  R1,447.59 │      -      │      R0.00    │
──────┴─────────────┴────────────┴─────────────┴────────────────┘

Total Payable: R17,371.08
Total Interest: R2,371.08
```

---

## Project Structure

```
LendFlow/
├── LendFlow.slnx
├── spec.md                          # Technical specification
├── README.md                        # This file
│
├── LendFlow.Api/                   # Entry point
│   ├── Controllers/
│   │   ├── ApplicantsController.cs
│   │   ├── ApplicationsController.cs
│   │   └── LoansController.cs
│   ├── Middleware/
│   │   ├── GlobalExceptionMiddleware.cs
│   │   └── TenantResolutionMiddleware.cs
│   ├── Models/
│   │   ├── CreateApplicantRequest.cs
│   │   ├── SubmitApplicationRequest.cs
│   │   ├── MakeDecisionRequest.cs
│   │   └── LoanModels.cs
│   ├── Program.cs
│   └── appsettings.json
│
├── LendFlow.Application/           # CQRS commands & queries
│   ├── Commands/
│   │   ├── CreateApplicant/
│   │   ├── SubmitApplication/
│   │   ├── AssessCredit/
│   │   ├── MakeDecision/
│   │   ├── DisburseLoan/
│   │   └── RecordRepayment/
│   ├── Queries/
│   │   ├── GetApplicant/
│   │   ├── GetLoanApplication/
│   │   ├── GetLoanApplications/
│   │   ├── GetLoan/
│   │   ├── GetLoans/
│   │   └── GetRepayments/
│   ├── CreditScoring/
│   │   ├── CreditAssessmentService.cs
│   │   ├── DecisionEngine.cs
│   │   ├── EmploymentStatusFactor.cs
│   │   ├── IncomeStabilityFactor.cs
│   │   ├── DebtToIncomeFactor.cs
│   │   └── LoanAmountFactor.cs
│   ├── Common/
│   │   ├── Interfaces/
│   │   ├── Models/
│   │   └── Behaviours/
│   └── DependencyInjection.cs
│
├── LendFlow.Domain/                 # Domain entities & logic
│   ├── Entities/
│   │   ├── Applicant.cs
│   │   ├── LoanApplication.cs
│   │   ├── Loan.cs
│   │   ├── Repayment.cs
│   │   ├── CreditAssessment.cs
│   │   ├── AuditLog.cs
│   │   └── Tenant.cs
│   ├── Enums/
│   │   ├── LoanApplicationStatus.cs
│   │   ├── LoanStatus.cs
│   │   ├── RepaymentStatus.cs
│   │   └── RiskBand.cs
│   ├── Events/
│   │   └── *.cs
│   ├── Exceptions/
│   │   └── NotFoundException.cs
│   ├── Common/
│   │   └── BaseAuditableEntity.cs
│   └── ValueObjects/
│       └── SouthAfricanIdNumber.cs
│
├── LendFlow.Infrastructure/        # Data access & external services
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/
│   ├── Services/
│   │   ├── CurrentTenantService.cs
│   │   ├── CurrentUserService.cs
│   │   └── RedisIdempotencyService.cs
│   ├── Migrations/
│   └── DependencyInjection.cs
│
└── LendFlow.Tests/                  # Unit tests
    ├── Commands/
    ├── Queries/
    ├── Validators/
    └── Testing/
```

---

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- SQL Server (or Docker)
- Redis

### Setup

```bash
# Clone the repository
git clone https://github.com/DynamicKarabo/lendflow.git
cd lendflow

# Start infrastructure (SQL Server + Redis)
docker run -d --name lendflow-sql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=LendFlow@123" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest

docker run -d --name lendflow-redis -p 6379:6379 redis:alpine

# Navigate to project
cd LendFlow/LendFlow

# Apply migrations
dotnet ef database update

# Run the API
dotnet run --project LendFlow.Api

# Access Swagger UI
open http://localhost:5000/swagger
```

### Generate JWT Token

```python
import json, base64, time, hmac, hashlib

header = {"alg": "HS256", "typ": "JWT"}
payload = {
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "admin",
    "tenant_id": "YOUR-TENANT-ID",
    "iss": "lendflow",
    "aud": "lendflow-api",
    "exp": int(time.time()) + 3600
}

secret = "lendflow-dev-secret-key-minimum-32-chars"
signature = hmac.new(secret.encode(), 
    f"{base64.urlsafe_b64encode(json.dumps(header).encode()).decode().rstrip('=')}."
    f"{base64.urlsafe_b64encode(json.dumps(payload).encode()).decode().rstrip('=')}".encode(), 
    hashlib.sha256).digest()

token = f"{base64.urlsafe_b64encode(json.dumps(header).encode()).decode().rstrip('=')}." \
       f"{base64.urlsafe_b64encode(json.dumps(payload).encode()).decode().rstrip('=')}." \
       f"{base64.urlsafe_b64encode(signature).decode().rstrip('=')}"
print(token)
```

---

## Test the API

```bash
# Get a JWT token (use the script above)

# Create applicant
curl -X POST http://localhost:5000/api/v1/applicants \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "FirstName": "John",
    "LastName": "Doe",
    "IdNumber": "9001015009087",
    "PhoneNumber": "+27821234567",
    "Email": "john@example.com",
    "DateOfBirth": "1990-01-01",
    "EmploymentStatus": "Employed",
    "MonthlyIncome": 45000,
    "MonthlyExpenses": 15000,
    "IdempotencyKey": "app-001"
  }'

# Submit application
curl -X POST http://localhost:5000/api/v1/applications \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "ApplicantId": "APPLICANT-ID-FROM-ABOVE",
    "Amount": 15000,
    "TermMonths": 12,
    "Purpose": "WorkingCapital",
    "IdempotencyKey": "app-002"
  }'

# Assess credit (triggers auto-decision)
curl -X POST http://localhost:5000/api/v1/applications/APPLICATION-ID/assess \
  -H "Authorization: Bearer YOUR_TOKEN"

# Disburse approved loan
curl -X POST http://localhost:5000/api/v1/loans/LOAN-ID/disburse \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "Method": "bank_transfer",
    "AccountNumber": "123456789",
    "BankCode": "SBZAZJJJ",
    "IdempotencyKey": "disburse-001"
  }'

# Record repayment
curl -X POST http://localhost:5000/api/v1/loans/LOAN-ID/repayments/pay \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "Amount": 1448,
    "PaymentReference": "EFT-001",
    "IdempotencyKey": "repay-001"
  }'
```

---

## Compliance

| Standard | Implementation |
|----------|----------------|
| **POPIA** | PII fields encrypted at rest, 5-year retention policy, no PII in logs |
| **FICA** | SA ID validation (Luhn, DOB cross-check), audit trail |
| **NCA** | Affordability check (DTI ≤40%), minimum age 18, debt-to-income ratio |

---

## Technology Stack

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Technology Stack                               │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│   ASP.NET Core   │     │    Entity        │     │     Redis       │
│   (.NET 9.0)     │     │    Framework 9   │     │   (Idempotency) │
└────────┬─────────┘     └────────┬─────────┘     └────────┬─────────┘
         │                         │                         │
         │     ┌───────────────────┘                         │
         │     │                                             │
         ▼     ▼                                             ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            MediatR (CQRS)                                  │
│  Commands  │  Queries  │  Pipeline Behaviors  │  Domain Events              │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          FluentValidation                                   │
│              Applicant Validator  │  Application Validator                  │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Stateless (State Machines)                         │
│       LoanApplication  │  Loan  │  Repayment  │  Credit Assessment          │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              SQL Server                                      │
│                      (Multi-tenant, Global Query Filters)                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Run Tests

```bash
cd LendFlow/LendFlow
dotnet test
```

```
Test run for LendFlow.Tests/bin/Debug/net9.0/LendFlow.Tests.dll

Passed!  - Failed: 0, Passed: 8, Skipped: 0, Total: 8
```

---

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit changes (`git commit -m 'Add amazing feature'`)
4. Push to branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

---

## License

MIT License - see [LICENSE](LICENSE) file for details.

---

## Authors

Built with ❤️ by [DynamicKarabo](https://github.com/DynamicKarabo)

[![GitHub](https://img.shields.io/badge/GitHub-DynamicKarabo-green?style=social&logo=github)](https://github.com/DynamicKarabo)
