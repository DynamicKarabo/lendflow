<!-- markdownlint-disable-next-line -->
<div align="center">

# LendFlow

![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![License](https://img.shields.io/badge/license-MIT-blue)
![Status](https://img.shields.io/badge/status-Production%20Ready-green)

**Enterprise-grade micro-lending API for South African lenders**

[Architecture](#-architecture) • [Features](#-features) • [Quick Start](#-quick-start) • [API](#-api-endpoints) • [Tech Stack](#-tech-stack)

</div>

---

## Overview

LendFlow is a **multi-tenant backend API** for managing the complete lifecycle of personal micro-loans. Built with Clean Architecture + CQRS pattern, designed for South African lending institutions with **POPIA**, **FICA**, and **NCA** compliance built-in.

### Key Capabilities

- ✅ Full loan lifecycle management
- ✅ Automated credit scoring (300-850 scale)
- ✅ Rule-based decision engine
- ✅ Idempotent financial operations
- ✅ Multi-tenancy from day one
- ✅ Comprehensive audit trail

---

## Architecture

```mermaid
flowchart TB
    subgraph Clients
        A[Mobile App]
        B[Web Portal]
        C[External Systems]
    end

    subgraph API["API Layer"]
        D[Applicants Controller]
        E[Applications Controller]
        F[Loans Controller]
    end

    subgraph App["Application Layer (CQRS)"]
        G[Commands]
        H[Queries]
        I[Credit Scoring]
        J[Decision Engine]
    end

    subgraph Domain["Domain Layer"]
        K[Entities]
        L[State Machines]
        M[Domain Events]
        N[Value Objects]
    end

    subgraph Infra["Infrastructure"]
        O[EF Core / SQL Server]
        P[Redis / Dapper]
        Q[Hangfire Jobs]
        R[Service Bus]
    end

    A --> D
    B --> E
    C --> F
    D --> G
    E --> G
    F --> G
    G --> K
    H --> K
    I --> K
    K --> O
    K --> P
    G --> Q
    Q --> R
```

---

## Entity Relationship

```mermaid
erDiagram
    Tenant ||--o{ Applicant : has
    Tenant ||--o{ LoanApplication : has
    Tenant ||--o{ Loan : has
    Tenant ||--o{ Repayment : has
    Tenant ||--o{ AuditLog : has
    
    Applicant ||--o{ LoanApplication : submits
    LoanApplication ||--|| Loan : creates
    LoanApplication ||--o{ CreditAssessment : has
    Loan ||--o{ Repayment : has

    Tenant {
        uuid Id
        string Name
        string ApiKeyHash
        bool IsActive
    }

    Applicant {
        uuid Id
        uuid TenantId
        string FirstName
        string LastName
        string IdNumber
        string PhoneNumber
        string Email
        date DateOfBirth
        string EmploymentStatus
        decimal MonthlyIncome
        decimal MonthlyExpenses
    }

    LoanApplication {
        uuid Id
        uuid TenantId
        uuid ApplicantId
        decimal RequestedAmount
        int RequestedTermMonths
        string Status
        int CreditScore
        string RiskBand
    }

    Loan {
        uuid Id
        uuid TenantId
        uuid ApplicationId
        decimal PrincipalAmount
        decimal InterestRate
        decimal OutstandingBalance
        string Status
    }

    Repayment {
        uuid Id
        uuid LoanId
        int InstallmentNumber
        decimal AmountDue
        decimal AmountPaid
        date DueDate
        string Status
    }
```

---

## Loan Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> Submitted: Submit
    Submitted --> UnderReview: Credit Assessment
    Submitted --> Cancelled: Cancel
    UnderReview --> Approved: Approve
    UnderReview --> Rejected: Reject
    Approved --> PendingDisbursement: Auto
    PendingDisbursement --> Active: Disburse
    Active --> Settled: Paid Off
    Active --> Defaulted: 3+ Missed
    Defaulted --> WrittenOff: Write Off
    Settled --> [*]
```

---

## Credit Scoring Flow

```mermaid
flowchart LR
    subgraph Input
        A[Applicant Data]
        B[Application Data]
    end

    subgraph Factors
        C[Employment<br/>Status 25%]
        D[Income<br/>Stability 25%]
        E[Debt-to-Income<br/>30%]
        F[Loan Amount<br/>20%]
    end

    subgraph Output
        G[Raw Score<br/>0-100]
        H[Mapped to<br/>300-850]
        I[Risk Band]
    end

    A --> C
    A --> D
    B --> E
    B --> F
    C --> G
    D --> G
    E --> G
    F --> G
    G --> H
    H --> I
```

---

## Features

### Core Features

| Feature | Description |
|---------|-------------|
| **Multi-Tenancy** | Multiple lenders on single platform with tenant isolation |
| **Credit Scoring** | Rule-based scoring with 4 weighted factors |
| **Decision Engine** | Auto-approve/reject + manual underwriter workflow |
| **State Machines** | Explicit state transitions for LoanApplication, Loan, Repayment |
| **Idempotency** | Redis-based idempotent keys prevent duplicate operations |
| **Audit Trail** | Append-only AuditLog with full history |

### Compliance

| Standard | Implementation |
|----------|---------------|
| **POPIA** | PII encrypted at rest (AES-256), 5-year retention, no PII in logs |
| **FICA** | SA ID validation (Luhn, DOB, gender, citizenship), audit trail |
| **NCA** | Min age 18, affordability check (DTI ≤ 40%), credit assessment |

---

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- SQL Server 2022+ (or Docker)
- Redis 7.x

### Run with Docker

```bash
# Clone and navigate
cd LendFlow

# Start infrastructure
docker run -d --name lendflow-sql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=LendFlow@123" \
  -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest

docker run -d --name lendflow-redis -p 6379:6379 redis:alpine

# Run migrations and start API
dotnet ef database update
dotnet run --project LendFlow.Api
```

### Access

- **Swagger UI**: http://localhost:5147/swagger
- **Health Check**: http://localhost:5147/health

---

## API Endpoints

### Applicants

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/applicants` | Register new applicant |
| GET | `/api/v1/applicants/{id}` | Get applicant details |

### Loan Applications

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/applications` | Submit loan application |
| GET | `/api/v1/applications` | List applications |
| GET | `/api/v1/applications/{id}` | Get application details |
| POST | `/api/v1/applications/{id}/assess` | Trigger credit assessment |
| POST | `/api/v1/applications/{id}/decision` | Manual decision |
| POST | `/api/v1/applications/{id}/cancel` | Cancel application |

### Loans

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/loans` | List loans |
| GET | `/api/v1/loans/{id}` | Get loan details |
| POST | `/api/v1/loans/{id}/disburse` | Disburse loan |
| GET | `/api/v1/loans/{id}/repayments` | Get repayment schedule |
| POST | `/api/v1/loans/{id}/repayments/pay` | Record repayment |

---

## Tech Stack

```mermaid
flowchart TB
    subgraph Frontend["Presentation"]
        A[Swagger UI]
    end

    subgraph API["API Layer"]
        B[ASP.NET Core 9.0<br/>Minimal APIs]
        C[JWT Auth]
    end

    subgraph App["Application"]
        D[MediatR 14.x<br/>CQRS]
        E[FluentValidation]
        F[Stateless<br/>State Machines]
    end

    subgraph Data["Data Layer"]
        G[EF Core 9.0<br/>Write]
        H[Dapper<br/>Read]
        I[SQL Server]
    end

    subgraph Infra["Infrastructure"]
        J[Redis<br/>Cache/Idempotency]
        K[Hangfire<br/>Background Jobs]
        L[Azure Service Bus<br/>Events]
        M[Serilog<br/>Logging]
        N[OpenTelemetry<br/>Tracing]
    end

    A --> B
    B --> C
    B --> D
    D --> E
    D --> F
    D --> G
    G --> I
    G --> H
    I --> J
    D --> K
    K --> L
    B --> M
    B --> N
```

| Component | Technology |
|-----------|------------|
| Framework | ASP.NET Core 9.0 |
| Language | C# / .NET 9.0 |
| ORM | Entity Framework Core 9.0 |
| Query | Dapper |
| Database | SQL Server |
| Cache | Redis |
| CQRS | MediatR 14.x |
| Validation | FluentValidation |
| State Machines | Stateless |
| Background Jobs | Hangfire |
| Messaging | Azure Service Bus |
| Auth | JWT Bearer |

---

## Project Structure

```
LendFlow/
├── LendFlow.slnx
├── appsettings.json
├── README.md
├── spec.md
├── DOCUMENTATION.md
│
├── LendFlow.Api/                # Entry point
│   ├── Controllers/
│   ├── Middleware/
│   ├── Models/
│   └── Program.cs
│
├── LendFlow.Application/       # CQRS layer
│   ├── Commands/
│   ├── Queries/
│   ├── CreditScoring/
│   └── Common/
│
├── LendFlow.Domain/           # Domain logic
│   ├── Entities/
│   ├── Enums/
│   ├── Events/
│   └── ValueObjects/
│
├── LendFlow.Infrastructure/    # Data & external
│   ├── Persistence/
│   ├── Services/
│   └── Migrations/
│
└── LendFlow.Tests/            # Unit tests
    ├── Commands/
    ├── Queries/
    └── Validators/
```

---

## Testing

```bash
cd LendFlow
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

MIT License - See [LICENSE](LICENSE) file for details.

---

## Authors

Built with ❤️ by [DynamicKarabo](https://github.com/DynamicKarabo)

[![GitHub](https://img.shields.io/badge/GitHub-DynamicKarabo-green?style=social&logo=github)](https://github.com/DynamicKarabo)

