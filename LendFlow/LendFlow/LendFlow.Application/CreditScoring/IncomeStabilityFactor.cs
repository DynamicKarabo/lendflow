using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;

namespace LendFlow.Application.CreditScoring;

public class IncomeStabilityFactor : ICreditScoringFactor
{
    public string Name => "IncomeStability";
    public int Weight => 25;

    public Task<CreditScoreFactorResult> EvaluateAsync(Applicant applicant, LoanApplication application, CancellationToken ct = default)
    {
        var income = applicant.MonthlyIncome;
        var score = income switch
        {
            >= 50000 => 100,
            >= 35000 => 85,
            >= 25000 => 70,
            >= 15000 => 50,
            >= 10000 => 30,
            >= 5000 => 15,
            _ => 0
        };

        return Task.FromResult(new CreditScoreFactorResult(
            FactorName: Name,
            Score: score,
            MaxScore: 100,
            Reason: $"Monthly income: R{income:N0}"
        ));
    }
}
