using System;
using LendFlow.Application.Common.Models;
using LendFlow.Domain.Enums;
using MediatR;

namespace LendFlow.Application.Queries.GetLoanApplications;

public record LoanApplicationListItemDto(
    Guid Id,
    Guid ApplicantId,
    string ApplicantName,
    decimal RequestedAmount,
    int RequestedTermMonths,
    string Purpose,
    LoanApplicationStatus Status,
    int? CreditScore,
    string? RiskBand,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record GetLoanApplicationsQuery(
    LoanApplicationStatus? Status,
    int PageNumber,
    int PageSize
) : IRequest<PagedResult<LoanApplicationListItemDto>>;
