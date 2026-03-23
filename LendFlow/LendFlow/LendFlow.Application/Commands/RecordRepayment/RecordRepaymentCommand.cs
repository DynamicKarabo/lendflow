using MediatR;

namespace LendFlow.Application.Commands.RecordRepayment;

public record RecordRepaymentCommand(
    Guid TenantId,
    Guid LoanId,
    decimal Amount,
    string PaymentReference,
    string IdempotencyKey
) : IRequest<RecordRepaymentResult>;
