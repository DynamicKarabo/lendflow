using LendFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace LendFlow.Infrastructure.Services.Stubs;

public class StubPaymentProcessor : IPaymentProcessor
{
    private readonly ILogger<StubPaymentProcessor> _logger;

    public StubPaymentProcessor(ILogger<StubPaymentProcessor> logger)
    {
        _logger = logger;
    }

    public Task<PaymentResult> ProcessDisbursementAsync(DisbursementRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Disbursement stub: Loan {LoanId}, Amount {Amount}, Account {Account}", 
            request.LoanId, request.Amount, request.AccountNumber[^4..]);
        
        return Task.FromResult(new PaymentResult(
            IsSuccess: true,
            TransactionId: Guid.NewGuid().ToString(),
            ProcessedAt: DateTime.UtcNow
        ));
    }

    public Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Payment stub: Loan {LoanId}, Amount {Amount}, Reference {Reference}", 
            request.LoanId, request.Amount, request.PaymentReference);
        
        return Task.FromResult(new PaymentResult(
            IsSuccess: true,
            TransactionId: Guid.NewGuid().ToString(),
            ProcessedAt: DateTime.UtcNow
        ));
    }
}
