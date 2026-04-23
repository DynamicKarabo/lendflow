using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.CreditScoring;
using LendFlow.Domain.Entities;
using LendFlow.Tests.Testing;
using Xunit;

namespace LendFlow.Tests.CreditScoring;

public class DecisionEngineTests
{
    private readonly DecisionEngine _engine = new();

    private Applicant CreateApplicant(DateOnly dob, decimal monthlyIncome)
    {
        var idNumber = TestData.CreateValidSaId(dob);
        return Applicant.Create(Guid.NewGuid(), "Name", "Surname", idNumber, "0800000000", "test@example.com", dob, "Employed", monthlyIncome, 0m);
    }

    private LoanApplication CreateApplication(Guid applicantId, decimal amount, int term)
    {
        return LoanApplication.Create(Guid.NewGuid(), applicantId, amount, term, "Test", Guid.NewGuid().ToString());
    }

    [Fact]
    public async Task EvaluateAsync_IncomeBelow2500_Rejects()
    {
        var dob = new DateOnly(1990, 1, 1);
        var applicant = CreateApplicant(dob, 2400m);
        var application = CreateApplication(applicant.Id, 1000m, 12);

        var result = await _engine.EvaluateAsync(applicant, application, 700, "Low", CancellationToken.None);

        Assert.False(result.IsApproved);
        Assert.Equal("Rejected", result.Decision);
        Assert.Contains(result.RejectionReasons, r => r.Contains("income below minimum threshold"));
    }

    [Fact]
    public async Task EvaluateAsync_DtiAbove40_Rejects()
    {
        var dob = new DateOnly(1990, 1, 1);
        var applicant = CreateApplicant(dob, 5000m);
        var application = CreateApplication(applicant.Id, 25000m, 6);

        var result = await _engine.EvaluateAsync(applicant, application, 700, "Low", CancellationToken.None);

        Assert.False(result.IsApproved);
        Assert.Equal("Rejected", result.Decision);
        Assert.Contains(result.RejectionReasons, r => r.Contains("exceeds 40%"));
    }

    [Fact]
    public async Task EvaluateAsync_Under18_Rejects()
    {
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-17)); 
        var applicant = (Applicant)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Applicant));
        
        var propDob = typeof(Applicant).GetProperty("DateOfBirth");
        propDob.SetValue(applicant, dob);

        var propIncome = typeof(Applicant).GetProperty("MonthlyIncome");
        propIncome.SetValue(applicant, 10000m);

        var application = CreateApplication(Guid.NewGuid(), 1000m, 12);

        var result = await _engine.EvaluateAsync(applicant, application, 700, "Low", CancellationToken.None);

        Assert.False(result.IsApproved);
        Assert.Equal("Rejected", result.Decision);
        Assert.Contains(result.RejectionReasons, r => r.Contains("under 18 years old"));
    }

    [Fact]
    public async Task EvaluateAsync_ScoreAbove650_Approves()
    {
        var dob = new DateOnly(1990, 1, 1);
        var applicant = CreateApplicant(dob, 10000m);
        var application = CreateApplication(applicant.Id, 1000m, 12);

        var result = await _engine.EvaluateAsync(applicant, application, 651, "Low", CancellationToken.None);

        Assert.True(result.IsApproved);
        Assert.Equal("Approved", result.Decision);
    }

    [Fact]
    public async Task EvaluateAsync_ScoreBetween550And650_ReturnsUnderReview()
    {
        var dob = new DateOnly(1990, 1, 1);
        var applicant = CreateApplicant(dob, 10000m);
        var application = CreateApplication(applicant.Id, 1000m, 12);

        var result = await _engine.EvaluateAsync(applicant, application, 600, "Medium", CancellationToken.None);

        Assert.False(result.IsApproved);
        Assert.Equal("UnderReview", result.Decision);
    }

    [Fact]
    public async Task EvaluateAsync_ScoreBelow550_Rejects()
    {
        var dob = new DateOnly(1990, 1, 1);
        var applicant = CreateApplicant(dob, 10000m);
        var application = CreateApplication(applicant.Id, 1000m, 12);

        var result = await _engine.EvaluateAsync(applicant, application, 500, "High", CancellationToken.None);

        Assert.False(result.IsApproved);
        Assert.Equal("Rejected", result.Decision);
        Assert.Contains(result.RejectionReasons, r => r.Contains("too low"));
    }
}
