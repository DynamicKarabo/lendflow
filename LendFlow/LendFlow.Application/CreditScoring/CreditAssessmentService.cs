using System.Text.Json;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;

namespace LendFlow.Application.CreditScoring;

public class CreditAssessmentService
{
    private readonly IEnumerable<ICreditScoringFactor> _factors;

    public CreditAssessmentService(IEnumerable<ICreditScoringFactor> factors)
    {
        _factors = factors;
    }

    public async Task<(int TotalScore, string RiskBand, List<CreditScoreFactorResult> Breakdown)> AssessAsync(
        Applicant applicant, 
        LoanApplication application, 
        CancellationToken ct = default)
    {
        var results = new List<CreditScoreFactorResult>();
        var weightedScore = 0;
        var totalWeight = 0;

        foreach (var factor in _factors)
        {
            var result = await factor.EvaluateAsync(applicant, application, ct);
            results.Add(result);
            
            weightedScore += (result.Score * factor.Weight);
            totalWeight += factor.Weight;
        }

        var normalizedScore = totalWeight > 0 ? (weightedScore / totalWeight) : 0;
        var scaledScore = MapToCreditScore(normalizedScore);
        var riskBand = DetermineRiskBand(scaledScore);

        return (scaledScore, riskBand, results);
    }

    private static int MapToCreditScore(int normalizedScore)
    {
        return normalizedScore switch
        {
            >= 90 => 800 + (normalizedScore - 90),
            >= 80 => 750 + (normalizedScore - 80) * 5,
            >= 70 => 700 + (normalizedScore - 70) * 5,
            >= 60 => 650 + (normalizedScore - 60) * 5,
            >= 50 => 600 + (normalizedScore - 50) * 5,
            >= 40 => 550 + (normalizedScore - 40) * 5,
            >= 30 => 500 + (normalizedScore - 30) * 5,
            >= 20 => 450 + (normalizedScore - 20) * 5,
            _ => 400 + (normalizedScore * 2)
        };
    }

    public static string DetermineRiskBand(int score)
    {
        return score switch
        {
            > 650 => "Low",
            >= 550 => "Medium",
            _ => "High"
        };
    }

    public static string SerializeBreakdown(List<CreditScoreFactorResult> breakdown)
    {
        return JsonSerializer.Serialize(breakdown);
    }
}
