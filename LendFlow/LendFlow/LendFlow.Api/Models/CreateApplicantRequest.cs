namespace LendFlow.Api.Models;

public record CreateApplicantRequest(
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
);
