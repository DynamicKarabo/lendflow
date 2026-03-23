namespace LendFlow.Application.Commands.MakeDecision;

public record MakeDecisionResult(
    Guid ApplicationId,
    string Decision,
    Guid? LoanId
);
