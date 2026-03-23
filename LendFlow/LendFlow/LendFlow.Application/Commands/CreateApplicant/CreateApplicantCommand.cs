using System;
using MediatR;

namespace LendFlow.Application.Commands.CreateApplicant;

public record CreateApplicantResult(Guid ApplicantId);

public record CreateApplicantCommand(
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
    string IdempotencyKey
) : IRequest<CreateApplicantResult>;
