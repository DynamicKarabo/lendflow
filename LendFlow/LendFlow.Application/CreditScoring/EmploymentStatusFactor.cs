using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;

namespace LendFlow.Application.CreditScoring;

public class EmploymentStatusFactor : ICreditScoringFactor
{
    public string Name => "EmploymentStatus";
    public int Weight => 25;

    public Task<CreditScoreFactorResult> EvaluateAsync(Applicant applicant, LoanApplication application, CancellationToken ct = default)
    {
        var score = applicant.EmploymentStatus.ToLower() switch
        {
            "employed" => 100,
            "selfemployed" => 75,
            "unemployed" => 25,
            _ => 0
        };

        return Task.FromResult(new CreditScoreFactorResult(
            FactorName: Name,
            Score: score,
            MaxScore: 100,
            Reason: $"Employment status: {applicant.EmploymentStatus}"
        ));
    }
}
