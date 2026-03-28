namespace LendFlow.Application.Commands.RecordRepayment;

public record RecordRepaymentResult(
    Guid LoanId,
    decimal AmountPaid,
    decimal OutstandingBalance,
    string LoanStatus,
    int RepaymentsRemaining
);
