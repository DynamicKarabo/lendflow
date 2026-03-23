namespace LendFlow.Application.Commands.AssessCredit;

public record AssessCreditResult(
    Guid AssessmentId,
    int Score,
    string RiskBand,
    string Decision,
    string Reason,
    List<string> FactorBreakdown
);
