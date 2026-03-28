using System.Text.Json;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using MediatR;

namespace LendFlow.Application.Commands.RecordRepayment;

public class RecordRepaymentCommandHandler : IRequestHandler<RecordRepaymentCommand, RecordRepaymentResult>
{
    private readonly IAppDbContext _context;
    private readonly IIdempotencyService _idempotencyService;

    public RecordRepaymentCommandHandler(IAppDbContext context, IIdempotencyService idempotencyService)
    {
        _context = context;
        _idempotencyService = idempotencyService;
    }

    public async Task<RecordRepaymentResult> Handle(RecordRepaymentCommand request, CancellationToken ct)
    {
        var existingJson = await _idempotencyService.GetStoredResultAsync(request.IdempotencyKey);
        if (!string.IsNullOrEmpty(existingJson))
        {
            return JsonSerializer.Deserialize<RecordRepaymentResult>(existingJson)!;
        }

        var loan = await _context.GetLoanAsync(request.LoanId, ct);
        if (loan == null)
            throw new InvalidOperationException($"Loan {request.LoanId} not found.");

        if (loan.Status != LoanStatus.Active)
            throw new InvalidOperationException($"Loan must be Active. Current: {loan.Status}");

        var repayments = await _context.GetRepaymentsByLoanIdAsync(request.LoanId, ct);
        var nextScheduled = repayments
            .Where(r => r.Status == RepaymentStatus.Scheduled || r.Status == RepaymentStatus.Late)
            .OrderBy(r => r.InstallmentNumber)
            .FirstOrDefault();

        if (nextScheduled == null)
            throw new InvalidOperationException("No scheduled repayments found.");

        if (request.Amount < nextScheduled.AmountDue)
            throw new InvalidOperationException($"Payment must be at least {nextScheduled.AmountDue}. Received: {request.Amount}");

        nextScheduled.RecordPayment(request.Amount, request.PaymentReference);

        var previousBalance = loan.OutstandingBalance;
        loan.RecordRepayment(nextScheduled.AmountDue);

        var auditLog = AuditLog.Create(
            request.TenantId,
            "Repayment",
            nextScheduled.Id,
            "Paid",
            $"Repayment of {request.Amount} recorded",
            "system",
            previousState: RepaymentStatus.Scheduled.ToString(),
            metadata: $"{{\"loanId\": \"{loan.Id}\", \"previousBalance\": {previousBalance}, \"newBalance\": {loan.OutstandingBalance}}}"
        );
        _context.AddAuditLog(auditLog);

        await _context.SaveChangesAsync(ct);

        var remainingRepayments = repayments.Count(r => 
            r.Status == RepaymentStatus.Scheduled || r.Status == RepaymentStatus.Late) - 1;

        var result = new RecordRepaymentResult(
            LoanId: loan.Id,
            AmountPaid: request.Amount,
            OutstandingBalance: loan.OutstandingBalance,
            LoanStatus: loan.Status.ToString(),
            RepaymentsRemaining: Math.Max(0, remainingRepayments)
        );

        await _idempotencyService.StoreResultAsync(request.IdempotencyKey, JsonSerializer.Serialize(result));

        return result;
    }
}
