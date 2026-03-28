namespace LendFlow.Application.Common.Interfaces;

public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessDisbursementAsync(DisbursementRequest request, CancellationToken ct = default);
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken ct = default);
}

public record DisbursementRequest(
    Guid LoanId,
    decimal Amount,
    string AccountNumber,
    string BankCode,
    string Reference
);

public record PaymentRequest(
    Guid LoanId,
    decimal Amount,
    string PaymentReference
);

public record PaymentResult(
    bool IsSuccess,
    string? TransactionId = null,
    string? ErrorMessage = null,
    DateTime? ProcessedAt = null
);
