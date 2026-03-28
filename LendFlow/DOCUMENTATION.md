# LendFlow - Enterprise Micro-Lending Credit Application API

**Version:** 1.0  
**Last Updated:** March 2026  
**Status:** Production Ready

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Technology Stack](#2-technology-stack)
3. [Architecture](#3-architecture)
4. [Domain Model](#4-domain-model)
5. [Core Features & Business Logic](#5-core-features--business-logic)
6. [API Reference](#6-api-reference)
7. [Data Models & Database Schema](#7-data-models--database-schema)
8. [State Machines](#8-state-machines)
9. [Credit Scoring Engine](#9-credit-scoring-engine)
10. [Decision Engine](#10-decision-engine)
11. [Multi-Tenancy](#11-multi-tenancy)
12. [Security & Compliance](#12-security--compliance)
13. [Idempotency](#13-idempotency)
14. [Background Jobs](#14-background-jobs)
15. [Domain Events](#15-domain-events)
16. [Configuration](#16-configuration)
17. [Getting Started](#17-getting-started)
18. [Testing](#18-testing)
19. [Observability](#19-observability)
20. [External Integrations](#20-external-integrations)

---

## 1. Project Overview

### 1.1 What is LendFlow?

**LendFlow** is an enterprise-grade, multi-tenant backend API designed to manage the complete lifecycle of personal micro-loans. Built specifically for South African lending institutions, it provides a robust, compliant, and scalable platform for processing credit applications from submission through disbursement and repayment.

### 1.2 Core Capabilities

| Capability | Description |
|------------|-------------|
| **Loan Application Management** | Full lifecycle from draft to approved/rejected |
| **Automated Credit Scoring** | Rule-based scoring engine (300-850 scale) |
| **Decision Engine** | Automatic and manual decisioning workflow |
| **Loan Disbursement** | Bank transfer execution with idempotency |
| **Repayment Tracking** | Schedule generation and payment recording |
| **Audit Trail** | Comprehensive append-only audit logging |
| **Multi-Tenancy** | Multiple lenders on single platform |
| **Regulatory Compliance** | POPIA, FICA, NCA compliance out-of-the-box |

### 1.3 Design Principles

1. **Correctness First** - Every state transition is explicit via state machines
2. **Idempotent Operations** - No duplicate disbursements or charges
3. **Auditability** - Every change recorded with actor, timestamp, and reason
4. **Multi-Tenant from Day One** - Tenant isolation at database level
5. **Compliance by Default** - South African regulatory requirements built-in

### 1.4 Target Users

| Role | Permissions |
|------|-------------|
| **Admin** | Full system access |
| **Underwriter** | Review and decision on pending applications |
| **System** | Internal service-to-service operations |

---

## 2. Technology Stack

### 2.1 Core Framework

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Technology Stack                                │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────┐     ┌────────────────────────────────────────┐
│         ASP.NET Core        │     │              .NET 9.0                 │
│         (Minimal APIs)      │     │         (LTS - Current)              │
└─────────────────────────────┘     └────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            MediatR 14.1.0                                    │
│                  CQRS Pattern - Commands & Queries                          │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    ▼                               ▼
┌─────────────────────────────────┐   ┌───────────────────────────────────────┐
│    Entity Framework Core 9.0    │   │              Dapper                   │
│         (Write Operations)      │   │          (Read Operations)             │
└─────────────────────────────────┘   └───────────────────────────────────────┘
```

### 2.2 Complete Technology Matrix

| Component | Technology | Version | Purpose |
|-----------|------------|---------|---------|
| **Runtime** | .NET | 9.0 | Core framework |
| **Web Framework** | ASP.NET Core | 9.0 | Minimal APIs |
| **ORM (Write)** | Entity Framework Core | 9.0 | Data persistence |
| **Query** | Dapper | 2.1.x | Optimized reads |
| **Database** | SQL Server | 2022 | Primary data store |
| **Cache/Idempotency** | Redis | 7.x | Distributed operations |
| **CQRS Mediator** | MediatR | 14.1.0 | Command/query dispatch |
| **Validation** | FluentValidation | 11.x | Request validation |
| **State Machines** | Stateless | 6.x | Explicit state transitions |
| **Background Jobs** | Hangfire | 1.8.x | Scheduled tasks |
| **Messaging** | Azure Service Bus | 2.x | Domain events |
| **Authentication** | JWT Bearer | Standard | Stateless auth |
| **Logging** | Serilog | 3.x | Structured logging |
| **Tracing** | OpenTelemetry | 1.x | Observability |
| **Testing** | xUnit | Latest | Unit testing |

### 2.3 Project Structure

```
LendFlow/
├── LendFlow.slnx                      # Solution file
├── spec.md                            # Technical specification
├── DOCUMENTATION.md                   # This file
│
├── LendFlow.Api/                      # Entry Point (Web API)
│   ├── Controllers/                   # API Controllers
│   │   ├── ApplicantsController.cs
│   │   ├── ApplicationsController.cs
│   │   └── LoansController.cs
│   ├── Middleware/                    # Custom Middleware
│   │   ├── GlobalExceptionMiddleware.cs
│   │   └── TenantResolutionMiddleware.cs
│   ├── Models/                        # Request/Response DTOs
│   │   ├── CreateApplicantRequest.cs
│   │   ├── SubmitApplicationRequest.cs
│   │   ├── MakeDecisionRequest.cs
│   │   └── LoanModels.cs
│   ├── Program.cs                     # App entry point
│   └── appsettings.json
│
├── LendFlow.Application/              # Application Layer (CQRS)
│   ├── Commands/                      # Command Handlers
│   │   ├── CreateApplicant/
│   │   ├── SubmitApplication/
│   │   ├── AssessCredit/
│   │   ├── MakeDecision/
│   │   ├── DisburseLoan/
│   │   └── RecordRepayment/
│   ├── Queries/                       # Query Handlers
│   │   ├── GetApplicant/
│   │   ├── GetLoanApplication/
│   │   ├── GetLoanApplications/
│   │   ├── GetLoan/
│   │   ├── GetLoans/
│   │   └── GetRepayments/
│   ├── CreditScoring/                 # Credit Assessment Engine
│   │   ├── CreditAssessmentService.cs
│   │   ├── DecisionEngine.cs
│   │   ├── EmploymentStatusFactor.cs
│   │   ├── IncomeStabilityFactor.cs
│   │   ├── DebtToIncomeFactor.cs
│   │   └── LoanAmountFactor.cs
│   ├── Events/                        # Domain Event Handlers
│   ├── Jobs/                          # Hangfire Background Jobs
│   ├── Common/
│   │   ├── Interfaces/               # Abstractions
│   │   ├── Models/                   # Shared Models
│   │   └── Behaviours/               # MediatR Pipeline
│   └── DependencyInjection.cs
│
├── LendFlow.Domain/                   # Domain Layer
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
│   │   ├── LoanApplicationTrigger.cs
│   │   ├── LoanStatus.cs
│   │   ├── LoanTrigger.cs
│   │   ├── RepaymentStatus.cs
│   │   └── RiskBand.cs
│   ├── Events/                        # Domain Events
│   ├── Exceptions/
│   │   └── NotFoundException.cs
│   ├── Common/
│   │   └── BaseAuditableEntity.cs
│   └── ValueObjects/
│       └── SouthAfricanIdNumber.cs
│
├── LendFlow.Infrastructure/           # Infrastructure Layer
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── AppDbContextFactory.cs
│   │   └── Configurations/           # EF Core Configurations
│   ├── Services/
│   │   ├── CurrentTenantService.cs
│   │   ├── CurrentUserService.cs
│   │   ├── RedisIdempotencyService.cs
│   │   ├── DomainEventDispatcher.cs
│   │   ├── ServiceBusPublisher.cs
│   │   └── Stubs/                    # Stub implementations
│   ├── Migrations/                   # EF Core Migrations
│   └── DependencyInjection.cs
│
└── LendFlow.Tests/                    # Unit Tests
    ├── Commands/
    ├── Queries/
    ├── Validators/
    └── Testing/
```

---

## 3. Architecture

### 3.1 Clean Architecture + CQRS

LendFlow follows **Clean Architecture** principles with **CQRS** (Command Query Responsibility Segregation) pattern:

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Layer Architecture                                  │
└─────────────────────────────────────────────────────────────────────────────┘

                              ┌─────────────┐
                              │   Clients   │
                              │ (Mobile/Web)│
                              └──────┬──────┘
                                     │
                              ┌──────▼──────┐
                              │  API Layer  │
                              │ (Minimal)   │
                              └──────┬──────┘
                                     │
              ┌──────────────────────┼──────────────────────┐
              │                      │                      │
       ┌──────▼──────┐        ┌──────▼──────┐        ┌──────▼──────┐
       │  Applicants │        │Applications │        │    Loans    │
       │  Controller │        │  Controller │        │  Controller │
       └──────┬──────┘        └──────┬──────┘        └──────┬──────┘
              │                      │                      │
              └──────────────────────┼──────────────────────┘
                                     │
                    ┌────────────────▼────────────────┐
                    │   Application Layer (CQRS)     │
                    │  ┌───────────┐  ┌───────────┐  │
                    │  │ Commands  │  │  Queries  │  │
                    │  │ (Writes)  │  │  (Reads)  │  │
                    │  └───────────┘  └───────────┘  │
                    │  ┌─────────────────────────┐   │
                    │  │ Pipeline Behaviors      │   │
                    │  │ - Validation            │   │
                    │  │ - Logging                │   │
                    │  │ - Exception Handling     │   │
                    │  └─────────────────────────┘   │
                    └────────────────┬────────────────┘
                                     │
                    ┌────────────────▼────────────────┐
                    │        Domain Layer            │
                    │  ┌─────┐ ┌─────┐ ┌─────┐      │
                    │  │Tenant│ │Loan │ │ Rep │      │
                    │  │Entity│ │State│ │aymnt│      │
                    │  └─────┘ └─────┘ └─────┘      │
                    │  ┌─────────────────────────┐  │
                    │  │ Value Objects           │  │
                    │  │ Domain Events           │  │
                    │  └─────────────────────────┘  │
                    └────────────────┬────────────────┘
                                     │
                    ┌────────────────▼────────────────┐
                    │   Infrastructure Layer        │
                    │  ┌────────┐ ┌────────┐        │
                    │  │  EF    │ │ Redis  │        │
                    │  │ Core 9 │ │ Cache  │        │
                    │  └────────┘ └────────┘        │
                    │  ┌─────────────────────────┐  │
                    │  │ External Services       │  │
                    │  │ (Stubbed for V1)        │  │
                    │  └─────────────────────────┘  │
                    └────────────────────────────────┘
```

### 3.2 Request Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Request Flow                                       │
└─────────────────────────────────────────────────────────────────────────────┘

  1. HTTP Request
         │
         ▼
  2. TenantResolutionMiddleware (extract tenant from JWT)
         │
         ▼
  3. Authentication/Authorization (JWT Bearer)
         │
         ▼
  4. Controller receives request
         │
         ▼
  5. MediatR Dispatches Command/Query
         │
         ▼
  6. Pipeline Behaviors (Validation, Logging)
         │
         ▼
  7. Handler executes business logic
         │
         ▼
  8. Domain Events published (if applicable)
         │
         ▼
  9. Response returned
         │
         ▼
 10. GlobalExceptionMiddleware catches errors → Problem Details
```

### 3.3 CQRS Pattern Implementation

**Commands (Write Operations):**
- CreateApplicantCommand
- SubmitApplicationCommand
- AssessCreditCommand
- MakeDecisionCommand
- DisburseLoanCommand
- RecordRepaymentCommand

**Queries (Read Operations):**
- GetApplicantQuery
- GetLoanApplicationQuery
- GetLoanApplicationsQuery
- GetLoanQuery
- GetLoansQuery
- GetRepaymentsQuery

### 3.4 MediatR Pipeline Behaviors

```
Request
    │
    ▼
┌─────────────────────────────┐
│  Logging Behavior           │  ← Logs request/response
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│  Validation Behavior        │  ← FluentValidation
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│  Performance Behavior        │  ← Timing metrics
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│  Exception Handling         │  ← Catches domain exceptions
└─────────────┬───────────────┘
              │
              ▼
         Handler
              │
              ▼
         Response
```

---

## 4. Domain Model

### 4.1 Entity Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Entity Relationship Diagram                         │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────┐         ┌──────────────────────┐         ┌─────────────────┐
│     Tenant      │         │   LoanApplication    │         │    Applicant   │
├─────────────────┤         ├──────────────────────┤         ├─────────────────┤
│ Id (PK)         │         │ Id (PK)              │         │ Id (PK)         │
│ Name            │         │ TenantId (FK)        │         │ TenantId (FK)   │
│ ApiKeyHash      │         │ ApplicantId (FK) ─────┼────────►│ FirstName       │
│ IsActive        │         │ RequestedAmount      │         │ LastName        │
│ CreatedAt       │         │ RequestedTermMonths  │         │ IdNumber (enc)  │
│ UpdatedAt       │         │ Purpose              │         │ PhoneNumber(enc)│
└─────────────────┘         │ Status (State)        │         │ Email           │
        │                  │ CreditScore          │         │ DateOfBirth     │
        │                  │ RiskBand             │         │ EmploymentStatus │
        │                  │ DecisionReason       │         │ MonthlyIncome   │
        │                  │ CreatedAt            │         │ MonthlyExpenses │
        │                  │ UpdatedAt            │         │ CreatedAt       │
        │                  └──────────┬───────────┘         │ UpdatedAt       │
        │                             │                      └─────────────────┘
        │                             │
        │                    ┌────────▼───────────┐
        │                    │ CreditAssessment  │
        │                    ├────────────────────┤
        │                    │ Id (PK)           │
        │                    │ ApplicationId(FK) │
        │                    │ Score (300-850)   │
        │                    │ RiskBand          │
        │                    │ FactorBreakdown   │
        │                    │ AssessedAt        │
        │                    └────────────────────┘
        │                             │
        │                    ┌────────▼───────────┐
        │                    │       Loan         │
        │                    ├────────────────────┤
        └───────────────────►│ Id (PK)           │
                            │ ApplicationId(FK) │
                            │ ApplicantId (FK)  │
                            │ PrincipalAmount    │
                            │ InterestRate      │
                            │ TermMonths        │
                            │ DisbursementDate  │
                            │ MaturityDate      │
                            │ OutstandingBal    │
                            │ Status (State)    │
                            │ CreatedAt         │
                            └────────┬───────────┘
                                     │
                            ┌────────▼───────────┐
                            │     Repayment       │
                            ├────────────────────┤
                            │ Id (PK)            │
                            │ LoanId (FK) ───────┘
                            │ InstallmentNumber  │
                            │ AmountDue          │
                            │ AmountPaid         │
                            │ DueDate            │
                            │ PaidDate           │
                            │ Status             │
                            │ PaymentReference   │
                            │ CreatedAt          │
                            └────────────────────┘

┌─────────────────┐
│    AuditLog     │  (Append-only)
├─────────────────┤
│ Id (PK)         │
│ TenantId (FK)   │
│ EntityType      │
│ EntityId        │
│ Action          │
│ PreviousState   │
│ NewState        │
│ PerformedBy     │
│ OccurredAt      │
│ Metadata (JSON) │
└─────────────────┘
```

### 4.2 Entity Definitions

#### 4.2.1 Tenant

Represents a lender/organization on the platform.

| Field | Type | Constraints | Description |
|-------|------|--------------|-------------|
| Id | UUID | PK | Unique identifier |
| Name | nvarchar(255) | Required | Tenant name |
| ApiKeyHash | nvarchar(255) | Required | Hashed API key for tenant resolution |
| IsActive | bit | Required | Soft delete flag |
| CreatedAt | datetimeoffset | Required | UTC creation timestamp |
| UpdatedAt | datetimeoffset | Optional | UTC last update timestamp |

#### 4.2.2 Applicant

Represents an individual applying for credit. **POPIA-compliant** with encrypted PII.

| Field | Type | Constraints | Description |
|-------|------|--------------|-------------|
| Id | UUID | PK | Unique identifier |
| TenantId | UUID | FK, Required | Multi-tenant identifier |
| FirstName | nvarchar(100) | Required | First name |
| LastName | nvarchar(100) | Required | Last name |
| IdNumber | nvarchar(255) | **Encrypted** | South African ID number |
| PhoneNumber | nvarchar(20) | **Encrypted** | Contact number |
| Email | nvarchar(255) | Required, Email | Email address |
| DateOfBirth | date | Required | Date of birth |
| EmploymentStatus | nvarchar(50) | Required | Employed/SelfEmployed/Unemployed |
| MonthlyIncome | decimal(18,4) | Required, > 0 | Monthly gross income |
| MonthlyExpenses | decimal(18,4) | Required, >= 0 | Monthly expenses |
| CreatedAt | datetimeoffset | Required | UTC creation timestamp |
| UpdatedAt | datetimeoffset | Optional | UTC last update timestamp |

**Business Rules:**
- SA ID number validated via Luhn algorithm
- DOB cross-validated against ID number date component
- Minimum age: 18 years (NCA requirement)
- IdNumber and PhoneNumber encrypted at rest with AES-256

#### 4.2.3 LoanApplication

Represents a credit application request. Uses **State Machine** for explicit transitions.

| Field | Type | Constraints | Description |
|-------|------|--------------|-------------|
| Id | UUID | PK | Unique identifier |
| TenantId | UUID | FK, Required | Multi-tenant identifier |
| ApplicantId | UUID | FK, Required | Reference to applicant |
| RequestedAmount | decimal(18,4) | Required, 100-50000 | Requested loan amount (ZAR) |
| RequestedTermMonths | int | Required, 1-60 | Loan term in months |
| Purpose | nvarchar(100) | Required | WorkingCapital/Education/Medical/Other |
| Status | nvarchar(50) | Required | Current state (see state machine) |
| CreditScore | int | Null | Score from assessment (300-850) |
| RiskBand | nvarchar(20) | Null | Low/Medium/High |
| DecisionReason | nvarchar(500) | Null | Approval/rejection reason |
| CreatedAt | datetimeoffset | Required | UTC creation timestamp |
| UpdatedAt | datetimeoffset | Optional | UTC last update timestamp |

#### 4.2.4 Loan

Created automatically when application is approved. Represents the active loan.

| Field | Type | Constraints | Description |
|-------|------|--------------|-------------|
| Id | UUID | PK | Unique identifier |
| TenantId | UUID | FK, Required | Multi-tenant identifier |
| ApplicationId | UUID | FK, Required | Source application |
| ApplicantId | UUID | FK, Required | Borrower reference |
| PrincipalAmount | decimal(18,4) | Required | Approved loan amount |
| InterestRate | decimal(8,4) | Required | Annual interest rate (e.g., 0.28 = 28%) |
| TermMonths | int | Required | Loan term in months |
| RepaymentFrequency | nvarchar(20) | Required | Monthly (V1 only) |
| DisbursementDate | datetimeoffset | Null | When funds were disbursed |
| MaturityDate | date | Null | Final payment due date |
| OutstandingBalance | decimal(18,4) | Required | Current outstanding amount |
| Status | nvarchar(50) | Required | Current state (see state machine) |
| CreatedAt | datetimeoffset | Required | UTC creation timestamp |

#### 4.2.5 Repayment

Individual installment record for a loan.

| Field | Type | Constraints | Description |
|-------|------|--------------|-------------|
| Id | UUID | PK | Unique identifier |
| TenantId | UUID | FK, Required | Multi-tenant identifier |
| LoanId | UUID | FK, Required | Parent loan |
| InstallmentNumber | int | Required | 1-based sequence number |
| AmountDue | decimal(18,4) | Required | Scheduled payment amount |
| AmountPaid | decimal(18,4) | Null | Actual amount paid |
| DueDate | date | Required | Scheduled due date |
| PaidDate | datetimeoffset | Null | Actual payment date |
| Status | nvarchar(20) | Required | Scheduled/Paid/Late/Missed |
| PaymentReference | nvarchar(255) | Null | External payment reference |
| CreatedAt | datetimeoffset | Required | UTC creation timestamp |

#### 4.2.6 CreditAssessment

Stores credit scoring results.

| Field | Type | Constraints | Description |
|-------|------|--------------|-------------|
| Id | UUID | PK | Unique identifier |
| TenantId | UUID | FK, Required | Multi-tenant identifier |
| ApplicationId | UUID | FK, Required | Assessed application |
| Score | int | Required | Final score (300-850) |
| RiskBand | nvarchar(20) | Required | Low/Medium/High |
| FactorBreakdown | nvarchar(max) | Required | JSON with factor scores |
| AssessedAt | datetimeoffset | Required | Assessment timestamp |

#### 4.2.7 AuditLog

**Append-only** audit trail. No UPDATE/DELETE operations permitted.

| Field | Type | Constraints | Description |
|-------|------|--------------|-------------|
| Id | UUID | PK | Unique identifier |
| TenantId | UUID | FK, Required | Multi-tenant identifier |
| EntityType | nvarchar(100) | Required | Entity type (e.g., "LoanApplication") |
| EntityId | UUID | Required | Entity identifier |
| Action | nvarchar(100) | Required | Action type (e.g., "StateTransition") |
| PreviousState | nvarchar(100) | Null | Previous state value |
| NewState | nvarchar(100) | Required | New state value |
| PerformedBy | nvarchar(255) | Required | Actor (user ID or "system") |
| OccurredAt | datetimeoffset | Required | Timestamp |
| Metadata | nvarchar(max) | Null | Additional JSON context |

---

## 5. Core Features & Business Logic

### 5.1 Loan Lifecycle Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Complete Loan Lifecycle                               │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────┐     ┌────────────┐     ┌─────────────┐     ┌─────────┐
│  Draft  │────►│ Submitted  │────►│ UnderReview  │────►│ Approved│
└─────────┘     └────────────┘     └──────┬───────┘     └────┬────┘
                                           │                  │
                                    ┌──────┴──────┐           │
                                    │             │           │
                                    ▼             ▼           ▼
                              ┌─────────┐   ┌─────────┐  ┌─────────────────────────┐
                              │ Rejected│   │ Cancelled│  │ PendingDisbursement     │
                              └─────────┘   └─────────┘  └───────────┬─────────────┘
                                                                         │
                                                                         │ Disburse
                                                                         ▼
                                                                 ┌─────────────────┐
                                                                 │     Active      │
                                                                 └────────┬────────┘
                                                                          │
                                                           ┌──────────────┼──────────────┐
                                                           │              │              │
                                                           ▼              ▼              ▼
                                                    ┌─────────┐  ┌───────────┐  ┌───────────┐
                                                    │ Settled │  │ Defaulted │  │ WrittenOff│
                                                    └─────────┘  └───────────┘  └───────────┘
```

### 5.2 Complete Workflow Steps

| Step | Action | Trigger | Result |
|------|--------|---------|--------|
| 1 | Register Applicant | POST /api/v1/applicants | Applicant record created |
| 2 | Submit Application | POST /api/v1/applications | Application in "Submitted" state |
| 3 | Credit Assessment | POST /api/v1/applications/{id}/assess | Score calculated (300-850) |
| 4 | Decision | Decision Engine | Status: Approved/Rejected/UnderReview |
| 5 | Manual Decision | POST /api/v1/applications/{id}/decision | Underwriter approval/rejection |
| 6 | Create Loan | Auto on approval | Loan in "PendingDisbursement" |
| 7 | Generate Schedule | Auto on loan creation | Repayment records created |
| 8 | Disburse Funds | POST /api/v1/loans/{id}/disburse | Loan in "Active" state |
| 9 | Record Repayment | POST /api/v1/loans/{id}/repayments/pay | Balance updated |
| 10 | Close Loan | Balance = 0 | Loan in "Settled" state |

### 5.3 Repayment Schedule Generation

**Formula: Equal Monthly Installments (EMI)**

```
EMI = P × [r(1+r)^n] / [(1+r)^n - 1]

Where:
  P = Principal loan amount
  r = Monthly interest rate (annual rate / 12)
  n = Number of months
```

**Example Calculation:**

| Input | Value |
|-------|-------|
| Principal | R15,000 |
| Annual Rate | 28% |
| Monthly Rate | 0.28 / 12 = 0.02333 |
| Term | 12 months |

```
EMI = 15000 × [0.02333 × (1.02333)^12] / [(1.02333)^12 - 1]
    = R1,447.59/month

Total Payable: R17,371.08
Total Interest: R2,371.08
```

### 5.4 South African ID Validation

**SA ID Number Structure:**
```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      SA ID Number Format (13 digits)                        │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────┬──────────┬──────┬─────┬────┬──────┐
│ YYYY      │ MM       │ DD   │ G   │ S  │  L   │
├──────────┼──────────┼──────┼─────┼────┼──────┤
│ Birth    │ Birth    │ Birth│Gender│Citi│ Luhn │
│ Year     │ Month    │ Day  │Digit │zen │Check │
└──────────┴──────────┴──────┴─────┴────┴──────┘
 4 digits   2 digits   2    1     1    1
```

**Validation Rules:**
1. **Length:** Must be exactly 13 digits
2. **Date Component:** MM (01-12), DD (01-31) must form valid date
3. **Gender:** 0-4 (Female), 5-9 (Male)
4. **Citizenship:** 0 (SA Citizen), 1 (Permanent Resident)
5. **Luhn Algorithm:** Valid checksum digit

---

## 6. API Reference

### 6.1 Base Configuration

| Setting | Value |
|---------|-------|
| Base URL | `/api/v1` |
| Authentication | JWT Bearer Token |
| Error Format | Problem Details (RFC 7807) |
| Content-Type | application/json |

### 6.2 Authentication

**Required JWT Claims:**
```json
{
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "admin",
  "tenant_id": "UUID",
  "iss": "lendflow",
  "aud": "lendflow-api",
  "exp": 1234567890
}
```

### 6.3 Endpoints

#### 6.3.1 Applicants

| Method | Endpoint | Description | Auth | Idempotent |
|--------|----------|-------------|------|------------|
| POST | `/api/v1/applicants` | Register new applicant | admin | Yes |
| GET | `/api/v1/applicants/{id}` | Get applicant details | admin, underwriter | N/A |

**POST /api/v1/applicants Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "idNumber": "9001015009087",
  "phoneNumber": "+27821234567",
  "email": "john@example.com",
  "dateOfBirth": "1990-01-01",
  "employmentStatus": "Employed",
  "monthlyIncome": 45000.00,
  "monthlyExpenses": 15000.00,
  "idempotencyKey": "uuid-string"
}
```

**Response (201 Created):**
```json
{
  "id": "uuid",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "dateOfBirth": "1990-01-01",
  "employmentStatus": "Employed",
  "monthlyIncome": 45000.00,
  "monthlyExpenses": 15000.00,
  "createdAt": "2026-03-28T10:00:00Z"
}
```

#### 6.3.2 Loan Applications

| Method | Endpoint | Description | Auth | Idempotent |
|--------|----------|-------------|------|------------|
| POST | `/api/v1/applications` | Submit application | admin | Yes |
| GET | `/api/v1/applications` | List applications | admin, underwriter | N/A |
| GET | `/api/v1/applications/{id}` | Get application | admin, underwriter | N/A |
| POST | `/api/v1/applications/{id}/assess` | Trigger assessment | admin, system | N/A |
| POST | `/api/v1/applications/{id}/decision` | Manual decision | underwriter, admin | N/A |
| POST | `/api/v1/applications/{id}/cancel` | Cancel application | admin | Yes |

**POST /api/v1/applications Request:**
```json
{
  "applicantId": "uuid",
  "amount": 15000.00,
  "termMonths": 12,
  "purpose": "WorkingCapital",
  "idempotencyKey": "uuid-string"
}
```

#### 6.3.3 Loans

| Method | Endpoint | Description | Auth | Idempotent |
|--------|----------|-------------|------|------------|
| GET | `/api/v1/loans` | List loans | admin | N/A |
| GET | `/api/v1/loans/{id}` | Get loan details | admin, underwriter | N/A |
| POST | `/api/v1/loans/{id}/disburse` | Disburse loan | admin | Yes |

**POST /api/v1/loans/{id}/disburse Request:**
```json
{
  "method": "bank_transfer",
  "accountNumber": "123456789",
  "bankCode": "SBZAZJJJ",
  "idempotencyKey": "uuid-string"
}
```

#### 6.3.4 Repayments

| Method | Endpoint | Description | Auth | Idempotent |
|--------|----------|-------------|------|------------|
| GET | `/api/v1/loans/{id}/repayments` | Get schedule | admin, underwriter | N/A |
| POST | `/api/v1/loans/{id}/repayments/pay` | Record payment | admin | Yes |

**POST /api/v1/loans/{id}/repayments/pay Request:**
```json
{
  "amount": 1447.59,
  "paymentReference": "EFT-ABC123",
  "idempotencyKey": "uuid-string"
}
```

#### 6.3.5 Health Checks

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Full health check (SQL + Redis) |
| GET | `/health/ready` | Readiness probe |
| GET | `/health/live` | Liveness probe |
| GET | `/hangfire` | Hangfire Dashboard (admin only) |

### 6.4 Error Response Format

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

---

## 7. Data Models & Database Schema

### 7.1 Database Indexes

| Table | Index | Columns | Purpose |
|-------|-------|---------|---------|
| LoanApplications | IX_LoanApplications_TenantId_Status | TenantId, Status | List filter |
| LoanApplications | IX_LoanApplications_TenantId_ApplicantId | TenantId, ApplicantId | Applicant lookup |
| Loans | IX_Loans_TenantId_Status | TenantId, Status | List filter |
| Loans | IX_Loans_ApplicationId | ApplicationId | FK lookup |
| Repayments | IX_Repayments_LoanId_Status | LoanId, Status | Schedule lookup |
| Repayments | IX_Repayments_DueDate_Status | DueDate, Status | Daily job |
| AuditLog | IX_AuditLog_EntityId_EntityType | EntityId, EntityType | Audit lookup |

### 7.2 Global Query Filters

All entities have a **tenant ID** and are filtered automatically via EF Core global query filters:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    builder.Entity<LoanApplication>()
        .HasQueryFilter(a => a.TenantId == _currentTenantService.TenantId);
}
```

---

## 8. State Machines

### 8.1 LoanApplication State Machine

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     Loan Application State Machine                          │
└─────────────────────────────────────────────────────────────────────────────┘

                          ┌─────────┐
                          │  Draft  │  ← Initial state when created
                          └────┬────┘
                               │ Submit
                               ▼
                    ┌─────────────────────┐
              ┌────►│     Submitted      │◄────┐
              │     └──────────┬──────────┘     │
              │              │                │ Cancel (by applicant)
              │              │                │ (before UnderReview)
              │              │ Credit Assess  │
              │              ▼                │
              │     ┌─────────────────┐        │
              │     │   UnderReview   │        │
              │     └────────┬────────┘        │
              │              │                 │
              │        ┌─────┴─────┐           │
              │        │           │           │
              │        ▼           ▼           │
              │  ┌─────────┐ ┌─────────┐      │
              │  │ Approved │ │ Rejected │      │
              │  └─────────┘ └─────────┘      │
              │        │                      │
              └────────┴──────────────────────┘

States: Draft, Submitted, UnderReview, Approved, Rejected, Cancelled
```

**Allowed Transitions:**
| From | Trigger | To |
|------|---------|-----|
| Draft | Submit | Submitted |
| Submitted | Assess | UnderReview |
| Submitted | Cancel | Cancelled |
| UnderReview | Approve | Approved |
| UnderReview | Reject | Rejected |
| Approved | (auto) | PendingDisbursement |

### 8.2 Loan State Machine

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Loan State Machine                                 │
└─────────────────────────────────────────────────────────────────────────────┘

              ┌─────────────────────────┐
              │  PendingDisbursement    │ ← Created on approval
              └───────────┬─────────────┘
                          │ Disburse
                          ▼
              ┌─────────────────────────┐
              │         Active          │◄──────────────────┐
              └───────────┬─────────────┘                   │
                          │                                  │
               ┌──────────┴──────────┐                      │
               │                     │                      │
               ▼                     ▼                      │
      ┌─────────────┐       ┌─────────────────┐            │
      │   Settled   │       │    Defaulted    │            │
      │ (Balance=0) │       │   (3+ missed)    │            │
      └─────────────┘       └────────┬─────────┘            │
                                       │                      │
                                       ▼                      │
                              ┌─────────────────┐            │
                              │    WrittenOff    │────────────┘
                              │   (Manual)       │
                              └─────────────────┘

States: PendingDisbursement, Active, Settled, Defaulted, WrittenOff
```

### 8.3 Repayment State Machine

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       Repayment State Machine                                │
└─────────────────────────────────────────────────────────────────────────────┘

      ┌──────────────┐
      │   Scheduled  │  ← Created with loan
      └──────┬───────┘
             │ Pay
             ▼
      ┌──────────────┐
      │     Paid     │  ← Payment recorded
      └──────────────┘

States: Scheduled, Paid
```

**Status Update Logic (Background Job):**
| Current | Condition | New Status |
|---------|-----------|------------|
| Scheduled | DueDate < Today - 3 days | Late |
| Scheduled | DueDate < Today | Missed |
| Late | Payment received | Paid |
| Missed | Payment received | Paid |

---

## 9. Credit Scoring Engine

### 9.1 Score Range

| Range | Risk Band | Decision |
|-------|------------|----------|
| 651-850 | Low | Auto-Approve |
| 550-650 | Medium | Under Review |
| 300-549 | High | Auto-Reject |

### 9.2 Scoring Factors

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     Credit Score Calculation Flow                            │
└─────────────────────────────────────────────────────────────────────────────┘

                              ┌─────────────────────────┐
                              │ CreditAssessmentService │
                              └───────────┬─────────────┘
                                          │
                                          ▼
                    ┌─────────────────────────────────────────────────────────┐
                    │              Scoring Factors                           │
                    └─────────────────────────────┬─────────────────────────┘
                                                  │
      ┌──────────────────────────────────────────┼──────────────────────────────────────────┐
      │                                          │                                          │
      ▼                                          ▼                                          ▼
┌───────────────────┐                  ┌───────────────────┐                          ┌───────────────────┐
│ Employment Status │                  │  Income Stability │                          │  Debt-to-Income   │
│    (25% weight)   │                  │   (25% weight)    │                          │   (30% weight)    │
├───────────────────┤                  ├───────────────────┤                          ├───────────────────┤
│ Employed    → 100│                  │ ≥R50,000 → 100   │                          │ DTI ≤10%  → 100  │
│ SelfEmployed→ 75 │                  │ ≥R35,000 → 85    │                          │ DTI ≤20%  → 85   │
│ Unemployed  → 25│                  │ ≥R25,000 → 70    │                          │ DTI ≤30%  → 70   │
│                    │                  │ ≥R15,000 → 55    │                          │ DTI ≤40%  → 50   │
└───────────────────┘                  └───────────────────┘                          │ DTI >40%  → 0    │
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
                                                  ▼
                                ┌───────────────────────────────────┐
                                │     Weighted Score Calculation    │
                                │                                    │
                                │ Score = (Emp×0.25 + Inc×0.25 +    │
                                │         DTI×0.30 + Loan×0.20)   │
                                └───────────────────┬───────────────┘
                                                    │
                                                    ▼
                                ┌───────────────────────────────────┐
                                │        Map to 300-850 Scale       │
                                ├───────────────────────────────────┤
                                │  90-100  → 800-850               │
                                │  80-89   → 750-799                │
                                │  70-79   → 700-749                │
                                │  60-69   → 650-699                │
                                │  50-59   → 600-649                │
                                │  40-49   → 550-599                │
                                │  30-39   → 500-549                │
                                │  20-29   → 450-499                │
                                │  0-19    → 400-449                │
                                └───────────────────────────────────┘
```

### 9.3 Factor Implementation

Each factor implements `ICreditScoringFactor` interface:

```csharp
public interface ICreditScoringFactor
{
    string Name { get; }
    double Weight { get; }
    int CalculateScore(LoanApplication application, Applicant applicant);
}
```

---

## 10. Decision Engine

### 10.1 Decision Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Decision Engine Flow                                 │
└─────────────────────────────────────────────────────────────────────────────┘

                    ┌───────────────────────┐
                    │ Application Submitted │
                    └───────────┬───────────┘
                                │
                                ▼
                    ┌───────────────────────┐
                    │  Hard Rejection Rules │
                    └───────────┬───────────┘
                                │
                    ┌───────────┴───────────┐
                    │                       │
                    ▼                       ▼
             ┌──────────┐           ┌──────────────┐
             │ REJECTED │           │ Continue to  │
             │ (Rule    │           │ Credit Score │
             │ Violation)│          │ Calculation  │
             └──────────┘           └──────────────┘
                                               │
                                               ▼
                                    ┌───────────────────────┐
                                    │   Calculate Score    │
                                    │      (300-850)       │
                                    └───────────┬───────────┘
                                                │
                                    ┌───────────┴───────────┐
                                    │                       │
                                    ▼                       ▼
                             ┌─────────────┐        ┌─────────────┐
                             │ Score > 650 │        │Score ≤ 650  │
                             │    (Auto)   │        │             │
                             └──────┬──────┘        └──────┬──────┘
                                    │                       │
                                    ▼                       ▼
                             ┌─────────────┐        ┌─────────────┐
                             │   APPROVED  │        │ UNDER REVIEW │
                             │  (Auto)     │        │   (Human)   │
                             └─────────────┘        └─────────────┘
```

### 10.2 Hard Rejection Rules

| Rule | Condition | Reason |
|------|-----------|--------|
| Minimum Age | Age < 18 | NCA requirement |
| Minimum Income | MonthlyIncome < R2,500 | Cannot afford |
| Affordability | DTI > 40% | NCA affordability |
| Existing Loan | Has Active Loan | Max 1 concurrent |

### 10.3 Score-Based Decisions

| Score Range | Risk Band | Action |
|-------------|------------|--------|
| 651-850 | Low | Auto-Approve |
| 550-650 | Medium | Under Review (manual decision required) |
| 300-549 | High | Auto-Reject |

---

## 11. Multi-Tenancy

### 11.1 Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       Multi-Tenant Architecture                              │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────┐  ┌─────────────────────┐  ┌─────────────────────┐
│      Tenant A       │  │      Tenant B       │  │      Tenant C       │
│   (Lender One)      │  │   (Lender Two)      │  │   (Lender Three)    │
├─────────────────────┤  ├─────────────────────┤  ├─────────────────────┤
│ Applicants          │  │ Applicants          │  │ Applicants          │
│ LoanApplications    │  │ LoanApplications    │  │ LoanApplications    │
│ Loans               │  │ Loans               │  │ Loans               │
│ Repayments          │  │ Repayments          │  │ Repayments          │
│ AuditLogs           │  │ AuditLogs           │  │ AuditLogs           │
└─────────────────────┘  └─────────────────────┘  └─────────────────────┘
            │                    │                    │
            └────────────────────┼────────────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │    Shared Database      │
                    │   (SQL Server)          │
                    │                         │
                    │  - All tenants in       │
                    │    single database      │
                    │  - TenantId on every    │
                    │    entity               │
                    │  - Global query filters │
                    │    enforce isolation    │
                    └─────────────────────────┘
```

### 11.2 Tenant Resolution

1. **JWT Token** contains `tenant_id` claim
2. **TenantResolutionMiddleware** extracts tenant ID from JWT
3. **CurrentTenantService** stores in AsyncLocal for request scope
4. **EF Core Query Filters** automatically filter by tenant

### 11.3 Tenant Isolation

- Every entity has `TenantId` foreign key
- EF Core global query filters applied to all entities
- No cross-tenant queries possible at data layer
- Audit logs include tenant ID for compliance

---

## 12. Security & Compliance

### 12.1 POPIA Compliance

| Requirement | Implementation |
|-------------|----------------|
| **Data Minimization** | Only required fields collected |
| **Encryption at Rest** | IdNumber, PhoneNumber encrypted with AES-256 |
| **No PII in Logs** | Structured logging excludes PII fields |
| **Retention Policy** | 5-year retention enforced by cleanup job |
| **No PII in Events** | Domain events use reference IDs only |

### 12.2 FICA Compliance

| Requirement | Implementation |
|-------------|----------------|
| **Identity Verification** | SA ID validation (Luhn, DOB, gender, citizenship) |
| **Audit Trail** | Append-only AuditLog table |
| **Non-Repudiation** | Every action recorded with actor and timestamp |

### 12.3 NCA Compliance

| Requirement | Implementation |
|-------------|----------------|
| **Minimum Age** | 18 years enforced at domain layer |
| **Affordability Assessment** | DTI ≤ 40% check before approval |
| **Credit Assessment** | Mandatory credit scoring before decision |
| **Responsible Lending** | Score-based and rule-based decisioning |

### 12.4 Data Encryption

```csharp
// PII fields encrypted using AES-256
public class EncryptionService
{
    private readonly byte[] _key; // From Azure Key Vault

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        return Convert.ToBase64String(aes.IV.Concat(cipherBytes).ToArray());
    }
}
```

---

## 13. Idempotency

### 13.1 Why Idempotency?

Prevents duplicate operations in case of:
- Network failures causing retry
- Client-side timeout then retry
- User double-clicking submit button

### 13.2 Implementation

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      Idempotency Implementation                              │
└─────────────────────────────────────────────────────────────────────────────┘

    Client Request                                                   Response
         │                                                               │
         │  ┌─────────────────────────────────────────────────────────┐  │
         │  │  1. Extract idempotency key from request                 │  │
         │  │     - Header: Idempotency-Key                           │  │
         │  │     - Body: idempotencyKey                              │  │
         │  └─────────────────────────────────────────────────────────┘  │
         │                                                               │
         │  ┌─────────────────────────────────────────────────────────┐  │
         │  │  2. Check Redis: SET NX key "idempotency:{key}"        │  │
         │  └─────────────────────────────────────────────────────────┘  │
         │                                                               │
         │  ┌──────────────────────┐     ┌──────────────────────────────┐│
         │  │  KEY NOT EXISTS      │     │  KEY EXISTS                  ││
         │  │  (First request)     │     │  (Duplicate request)         ││
         │  └──────────┬───────────┘     └──────────────┬───────────────┘│
         │             │                                │                │
         │             ▼                                ▼                │
         │  ┌──────────────────────┐     ┌──────────────────────────────┐│
         │  │  3. Execute business  │     │  Return cached response      ││
         │  │     logic             │     │  (same as original request)  ││
         │  │                       │     │                              ││
         │  │  4. Cache response    │     └──────────────────────────────┘│
         │  │     with key          │                                     │
         │  └──────────────────────┘                                     │
         │                                                               ▼
         └───────────────────────────────────────────────────────────────┘
```

### 13.3 Covered Endpoints

| Endpoint | Key Source |
|----------|------------|
| POST /api/v1/applicants | Request body `idempotencyKey` |
| POST /api/v1/applications | Request body `idempotencyKey` |
| POST /api/v1/loans/{id}/disburse | Request body `idempotencyKey` |
| POST /api/v1/loans/{id}/repayments/pay | Request body `idempotencyKey` |

---

## 14. Background Jobs

### 14.1 Hangfire Jobs

| Job | Schedule | Description |
|-----|----------|-------------|
| CreditAssessmentJob | Event-triggered | Run credit scoring on submitted application |
| RepaymentStatusJob | Daily 06:00 UTC | Mark late/missed repayments |
| RepaymentReminderJob | Daily 08:00 UTC | Send repayment reminders |
| RetentionCleanupJob | Weekly Sunday 03:00 UTC | Archive old records |

### 14.2 Job Implementation

```csharp
public class RepaymentStatusJob
{
    public async Task ExecuteAsync(AppDbContext context)
    {
        var threeDaysAgo = DateTime.UtcNow.AddDays(-3);
        
        var lateRepayments = await context.Repayments
            .Where(r => r.Status == RepaymentStatus.Scheduled)
            .Where(r => r.DueDate < threeDaysAgo)
            .ToListAsync();
        
        foreach (var repayment in lateRepayments)
        {
            repayment.Status = repayment.DueDate < DateTime.UtcNow 
                ? RepaymentStatus.Missed 
                : RepaymentStatus.Late;
        }
        
        await context.SaveChangesAsync();
    }
}
```

---

## 15. Domain Events

### 15.1 Event Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        Domain Events Flow                                    │
└─────────────────────────────────────────────────────────────────────────────┘

                    ┌─────────────────────┐
                    │   Domain Event      │
                    │   Published         │
                    └──────────┬──────────┘
                               │
                               ▼
              ┌──────────────────────────────────────┐
              │      Azure Service Bus               │
              │      (Message Broker)                │
              └──────────────┬───────────────────────┘
                             │
         ┌───────────────────┼───────────────────┐
         │                   │                   │
         ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│  Job Trigger    │ │ Notification    │ │  Balance        │
│  (Hangfire)     │ │ Service         │ │  Update         │
└─────────────────┘ └─────────────────┘ └─────────────────┘
```

### 15.2 Events

| Event | Published When | Consumers |
|-------|----------------|------------|
| application.submitted | Application moved to Submitted | CreditAssessmentJob |
| application.assessed | Credit score calculated | DecisionEngine |
| application.approved | Application approved | LoanCreationHandler |
| application.rejected | Application rejected | NotificationService |
| loan.created | Loan created | RepaymentScheduleGenerator |
| loan.disbursed | Disbursement confirmed | NotificationService |
| repayment.paid | Payment recorded | BalanceUpdateHandler |
| loan.closed | Balance = 0 | AuditService |

---

## 16. Configuration

### 16.1 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=LendFlow;User Id=sa;Password=LendFlow@123;TrustServerCertificate=True;",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Issuer": "lendflow",
    "Audience": "lendflow-api",
    "Key": "lendflow-dev-secret-key-minimum-32-chars"
  },
  "Serilog": {
    "MinimumLevel": "Information"
  },
  "AllowedHosts": "*"
}
```

### 16.2 Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| ConnectionStrings__DefaultConnection | SQL Server | Yes |
| ConnectionStrings__Redis | Redis connection | Yes |
| Jwt__Issuer | JWT issuer | Yes |
| Jwt__Audience | JWT audience | Yes |
| Jwt__Key | JWT signing key (min 32 chars) | Yes |

---

## 17. Getting Started

### 17.1 Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 9.0+ |
| SQL Server | 2022+ |
| Redis | 7.x |
| Docker | Latest (optional) |

### 17.2 Setup Steps

```bash
# 1. Clone repository
git clone https://github.com/DynamicKarabo/lendflow.git
cd lendflow

# 2. Start infrastructure (Docker)
docker run -d --name lendflow-sql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=LendFlow@123" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest

docker run -d --name lendflow-redis -p 6379:6379 redis:alpine

# 3. Navigate to project
cd LendFlow/LendFlow

# 4. Apply migrations
dotnet ef database update

# 5. Run the API
dotnet run --project LendFlow.Api

# 6. Access Swagger UI
open http://localhost:5000/swagger
```

### 17.3 Generate JWT Token

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

### 17.4 Example API Calls

```bash
# Create applicant
curl -X POST http://localhost:5000/api/v1/applicants \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "idNumber": "9001015009087",
    "phoneNumber": "+27821234567",
    "email": "john@example.com",
    "dateOfBirth": "1990-01-01",
    "employmentStatus": "Employed",
    "monthlyIncome": 45000,
    "monthlyExpenses": 15000,
    "idempotencyKey": "app-001"
  }'

# Submit application
curl -X POST http://localhost:5000/api/v1/applications \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "applicantId": "APPLICANT-ID",
    "amount": 15000,
    "termMonths": 12,
    "purpose": "WorkingCapital",
    "idempotencyKey": "app-002"
  }'

# Assess credit
curl -X POST http://localhost:5000/api/v1/applications/APPLICATION-ID/assess \
  -H "Authorization: Bearer YOUR_TOKEN"

# Disburse approved loan
curl -X POST http://localhost:5000/api/v1/loans/LOAN-ID/disburse \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "method": "bank_transfer",
    "accountNumber": "123456789",
    "bankCode": "SBZAZJJJ",
    "idempotencyKey": "disburse-001"
  }'

# Record repayment
curl -X POST http://localhost:5000/api/v1/loans/LOAN-ID/repayments/pay \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 1447.59,
    "paymentReference": "EFT-001",
    "idempotencyKey": "repay-001"
  }'
```

---

## 18. Testing

### 18.1 Run Tests

```bash
cd LendFlow/LendFlow
dotnet test
```

### 18.2 Test Coverage

| Category | Description |
|----------|-------------|
| Commands | Unit tests for command handlers |
| Queries | Unit tests for query handlers |
| Validators | Request validation tests |
| Domain | Entity and value object tests |

---

## 19. Observability

### 19.1 Logging (Serilog)

- Structured logging with JSON format
- Request/response logging with correlation IDs
- PII fields explicitly excluded from logs

### 19.2 Tracing (OpenTelemetry)

- Distributed tracing across services
- Automatic span creation for HTTP requests
- Custom spans for database operations

### 19.3 Health Checks

| Endpoint | Checks |
|----------|--------|
| /health | SQL Server, Redis |
| /health/ready | All dependencies |
| /health/live | Process is running |

---

## 20. External Integrations

### 20.1 Stubbed Services

All external integrations are stubbed for V1:

| Service | Interface | Stub Behavior |
|---------|-----------|---------------|
| KYC Provider | IKycProvider | Always returns verified |
| Credit Bureau | ICreditBureauProvider | Returns empty history |
| Payment Processor | IPaymentProcessor | Always returns success |
| Notifications | INotificationService | Logs to console |

### 20.2 Interface Definitions

```csharp
public interface IKycProvider
{
    Task<KycResult> VerifyIdentityAsync(string idNumber, string firstName, string lastName);
}

public interface ICreditBureauProvider
{
    Task<CreditReport> GetCreditReportAsync(string idNumber);
}

public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessDisbursementAsync(DisbursementRequest request);
    Task<PaymentResult> ProcessRepaymentAsync(RepaymentRequest request);
}

public interface INotificationService
{
    Task SendSmsAsync(string phoneNumber, string message);
    Task SendEmailAsync(string email, string subject, string body);
}
```

---

## Appendix A: State Transition Tables

### A.1 LoanApplication Transitions

| Current State | Trigger | New State | Condition |
|---------------|---------|------------|------------|
| Draft | Submit | Submitted | - |
| Submitted | Assess | UnderReview | - |
| Submitted | Cancel | Cancelled | - |
| UnderReview | Approve | Approved | Role: underwriter |
| UnderReview | Reject | Rejected | Role: underwriter |

### A.2 Loan Transitions

| Current State | Trigger | New State | Condition |
|---------------|---------|------------|------------|
| PendingDisbursement | Disburse | Active | - |
| Active | Payment (balance=0) | Settled | - |
| Active | 3+ missed | Defaulted | - |
| Defaulted | WriteOff | WrittenOff | Role: admin |

---

## Appendix B: Risk Band Definitions

| Risk Band | Score Range | Action | Interest Rate Impact |
|-----------|-------------|--------|---------------------|
| Low | 651-850 | Auto-approve | Base rate |
| Medium | 550-650 | Under review | +5% |
| High | 300-549 | Auto-reject | N/A |

---

## Appendix C: Compliance Checklist

### C.1 POPIA

- [x] Data minimization
- [x] Encryption at rest (AES-256)
- [x] No PII in logs
- [x] Retention policy (5 years)
- [x] No PII in events

### C.2 FICA

- [x] SA ID validation
- [x] Append-only audit
- [x] Actor tracking

### C.3 NCA

- [x] Minimum age 18
- [x] Affordability check (DTI ≤ 40%)
- [x] Credit assessment required
- [x] Score-based decisioning

---

## Appendix D: API Response Codes

| Code | Meaning |
|------|---------|
| 200 | Success |
| 201 | Created |
| 400 | Validation Error |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 409 | Conflict (duplicate idempotency key) |
| 422 | Business Rule Violation |
| 500 | Internal Server Error |

---

## Appendix E: Glossary

| Term | Definition |
|------|------------|
| **Applicant** | Person applying for credit |
| **LoanApplication** | Request for credit |
| **Loan** | Approved credit agreement |
| **Repayment** | Scheduled payment installment |
| **DTI** | Debt-to-Income ratio |
| **EMI** | Equal Monthly Installment |
| **POPIA** | Protection of Personal Information Act |
| **FICA** | Financial Intelligence Centre Act |
| **NCA** | National Credit Act |
| **CQRS** | Command Query Responsibility Segregation |
| **Idempotency** | Same request produces same result |

---

## License

MIT License - See LICENSE file for details.

## Authors

Built with care by [DynamicKarabo](https://github.com/DynamicKarabo)

---

*This documentation was last updated on March 28, 2026.*
