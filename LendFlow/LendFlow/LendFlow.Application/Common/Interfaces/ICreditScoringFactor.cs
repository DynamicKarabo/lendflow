using LendFlow.Domain.Entities;

namespace LendFlow.Application.Common.Interfaces;

public record CreditScoreFactorResult(string FactorName, int Score, int MaxScore, string Reason);

public interface ICreditScoringFactor
{
    string Name { get; }
    int Weight { get; }
    Task<CreditScoreFactorResult> EvaluateAsync(Applicant applicant, LoanApplication application, CancellationToken ct = default);
}
