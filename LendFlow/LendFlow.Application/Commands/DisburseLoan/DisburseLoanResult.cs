namespace LendFlow.Application.Commands.DisburseLoan;

public record DisburseLoanResult(
    Guid LoanId,
    string Status,
    DateTimeOffset DisbursementDate,
    decimal Amount,
    string Message
);
