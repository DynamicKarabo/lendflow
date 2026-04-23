using System;
using LendFlow.Domain.Entities;
using LendFlow.Tests.Testing;
using Xunit;

namespace LendFlow.Tests.Domain;

public class ApplicantTests
{
    [Fact]
    public void Create_ValidApplicant_CreatesSuccessfully()
    {
        var tenantId = Guid.NewGuid();
        var dob = new DateOnly(1990, 1, 15);
        var idNumber = TestData.CreateValidSaId(dob);

        var applicant = Applicant.Create(
            tenantId, "John", "Doe", idNumber, "0821234567",
            "john@example.com", dob, "Employed", 30000m, 15000m);

        Assert.NotNull(applicant);
        Assert.Equal(tenantId, applicant.TenantId);
        Assert.Equal("John", applicant.FirstName);
        Assert.Equal("Doe", applicant.LastName);
        Assert.Equal(idNumber, applicant.IdNumber);
        Assert.Equal("0821234567", applicant.PhoneNumber);
        Assert.Equal("john@example.com", applicant.Email);
        Assert.Equal(dob, applicant.DateOfBirth);
        Assert.Equal("Employed", applicant.EmploymentStatus);
        Assert.Equal(30000m, applicant.MonthlyIncome);
        Assert.Equal(15000m, applicant.MonthlyExpenses);
    }

    [Fact]
    public void Create_Under18Applicant_ThrowsInvalidOperationException()
    {
        var tenantId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dob = today.AddYears(-17);
        var idNumber = TestData.CreateValidSaId(dob);

        var exception = Assert.Throws<InvalidOperationException>(() => Applicant.Create(
            tenantId, "Young", "Person", idNumber, "0821234567",
            "young@example.com", dob, "Unemployed", 0m, 0m));

        Assert.Equal("Applicant must be 18 years or older.", exception.Message);
    }
}
