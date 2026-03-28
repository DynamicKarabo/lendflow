using MediatR;

namespace LendFlow.Application.Commands.DisburseLoan;

public record DisburseLoanCommand(
    Guid TenantId,
    Guid LoanId,
    string Method,
    string AccountNumber,
    string BankCode,
    string IdempotencyKey
) : IRequest<DisburseLoanResult>;
