using System;
using MediatR;
using LendFlow.Domain.Enums;

namespace LendFlow.Application.Queries.GetLoanApplication;

public record LoanApplicationDto(
    Guid Id,
    Guid TenantId,
    Guid ApplicantId,
    decimal RequestedAmount,
    int RequestedTermMonths,
    string Purpose,
    LoanApplicationStatus Status,
    int? CreditScore,
    string? RiskBand,
    string? DecisionReason,
    string IdempotencyKey,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record GetLoanApplicationQuery(Guid ApplicationId) : IRequest<LoanApplicationDto>;
