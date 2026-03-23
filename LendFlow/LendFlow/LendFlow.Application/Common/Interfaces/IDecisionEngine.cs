using LendFlow.Domain.Entities;

namespace LendFlow.Application.Common.Interfaces;

public record DecisionResult(
    bool IsApproved,
    string Decision,
    string Reason,
    int? Score,
    string? RiskBand,
    List<string> RejectionReasons);

public interface IDecisionEngine
{
    Task<DecisionResult> EvaluateAsync(Applicant applicant, LoanApplication application, int creditScore, string riskBand, CancellationToken ct = default);
}
