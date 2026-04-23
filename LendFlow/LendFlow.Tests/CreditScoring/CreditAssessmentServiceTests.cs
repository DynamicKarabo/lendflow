using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.CreditScoring;
using LendFlow.Domain.Entities;
using LendFlow.Tests.Testing;
using Xunit;

namespace LendFlow.Tests.CreditScoring;

public class CreditAssessmentServiceTests
{
    private CreditAssessmentService CreateService()
    {
        var factors = new List<ICreditScoringFactor>
        {
            new EmploymentStatusFactor(), 
            new IncomeStabilityFactor(), 
            new DebtToIncomeFactor(), 
            new LoanAmountFactor() 
        };
        return new CreditAssessmentService(factors);
    }

    [Fact]
    public async Task AssessAsync_HighIncomeEmployedApplicant_ReturnsHighScore()
    {
        var service = CreateService();
        var dob = new DateOnly(1980, 1, 1);
        var idNumber = TestData.CreateValidSaId(dob);
        var applicant = Applicant.Create(Guid.NewGuid(), "Jane", "Doe", idNumber, "0820000000", "jane@example.com", dob, "Employed", 50000m, 10000m);
        var application = LoanApplication.Create(Guid.NewGuid(), applicant.Id, 10000m, 12, "Car", Guid.NewGuid().ToString());

        var (score, riskBand, breakdown) = await service.AssessAsync(applicant, application, CancellationToken.None);

        Assert.True(score > 650, $"Score {score} was expected to be > 650");
        Assert.Equal("Low", riskBand);
    }

    [Fact]
    public async Task AssessAsync_UnemployedLowIncomeApplicant_ReturnsLowScore()
    {
        var service = CreateService();
        var dob = new DateOnly(1980, 1, 1);
        var idNumber = TestData.CreateValidSaId(dob);
        var applicant = Applicant.Create(Guid.NewGuid(), "John", "Doe", idNumber, "0820000000", "john@example.com", dob, "Unemployed", 0m, 1000m);
        var application = LoanApplication.Create(Guid.NewGuid(), applicant.Id, 5000m, 12, "Food", Guid.NewGuid().ToString());

        var (score, riskBand, breakdown) = await service.AssessAsync(applicant, application, CancellationToken.None);

        Assert.True(score < 550, $"Score {score} was expected to be < 550");
        Assert.Equal("High", riskBand);
    }

    [Theory]
    [InlineData(651, "Low")]
    [InlineData(800, "Low")]
    [InlineData(550, "Medium")]
    [InlineData(650, "Medium")]
    [InlineData(549, "High")]
    [InlineData(400, "High")]
    public void DetermineRiskBand_ReturnsCorrectBand(int score, string expectedBand)
    {
        var band = CreditAssessmentService.DetermineRiskBand(score);
        Assert.Equal(expectedBand, band);
    }
}
