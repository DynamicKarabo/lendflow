using System;
using LendFlow.Application.Commands.CreateApplicant;
using LendFlow.Application.Commands.SubmitApplication;
using LendFlow.Application.Queries.GetLoanApplications;
using LendFlow.Tests.Testing;
using Xunit;

namespace LendFlow.Tests.Validators;

public class ValidatorTests
{
    [Fact]
    public void CreateApplicantValidator_InvalidEmail_Fails()
    {
        var validator = new CreateApplicantCommandValidator();
        var dob = new DateOnly(1990, 1, 15);
        var command = new CreateApplicantCommand(
            TenantId: Guid.NewGuid(),
            FirstName: "John",
            LastName: "Doe",
            IdNumber: TestData.CreateValidSaId(dob),
            PhoneNumber: "0712345678",
            Email: "not-an-email",
            DateOfBirth: dob,
            EmploymentStatus: "Employed",
            MonthlyIncome: 10000m,
            MonthlyExpenses: 3000m,
            IdempotencyKey: "key-1"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void SubmitApplicationValidator_InvalidAmount_Fails()
    {
        var validator = new SubmitApplicationCommandValidator();
        var command = new SubmitApplicationCommand(
            TenantId: Guid.NewGuid(),
            ApplicantId: Guid.NewGuid(),
            RequestedAmount: 50m,
            RequestedTermMonths: 3,
            Purpose: "working_capital",
            IdempotencyKey: "key-2"
        );

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void GetLoanApplicationsValidator_InvalidPaging_Fails()
    {
        var validator = new GetLoanApplicationsQueryValidator();
        var query = new GetLoanApplicationsQuery(null, 0, 200);

        var result = validator.Validate(query);

        Assert.False(result.IsValid);
    }
}
