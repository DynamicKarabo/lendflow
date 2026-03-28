using System;
using MediatR;

namespace LendFlow.Application.Queries.GetApplicant;

public record ApplicantDto(
    Guid Id,
    Guid TenantId,
    string FirstName,
    string LastName,
    string IdNumber,
    string PhoneNumber,
    string Email,
    DateOnly DateOfBirth,
    string EmploymentStatus,
    decimal MonthlyIncome,
    decimal MonthlyExpenses,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record GetApplicantQuery(Guid ApplicantId) : IRequest<ApplicantDto>;
