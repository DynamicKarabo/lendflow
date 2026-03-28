using System.Text.Json;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using MediatR;

namespace LendFlow.Application.Commands.DisburseLoan;

public class DisburseLoanCommandHandler : IRequestHandler<DisburseLoanCommand, DisburseLoanResult>
{
    private readonly IAppDbContext _context;
    private readonly IIdempotencyService _idempotencyService;

    public DisburseLoanCommandHandler(IAppDbContext context, IIdempotencyService idempotencyService)
    {
        _context = context;
        _idempotencyService = idempotencyService;
    }

    public async Task<DisburseLoanResult> Handle(DisburseLoanCommand request, CancellationToken ct)
    {
        var existingJson = await _idempotencyService.GetStoredResultAsync(request.IdempotencyKey);
        if (!string.IsNullOrEmpty(existingJson))
        {
            return JsonSerializer.Deserialize<DisburseLoanResult>(existingJson)!;
        }

        var loan = await _context.GetLoanAsync(request.LoanId, ct);
        if (loan == null)
            throw new InvalidOperationException($"Loan {request.LoanId} not found.");

        if (loan.Status != LoanStatus.PendingDisbursement)
            throw new InvalidOperationException($"Loan must be in PendingDisbursement status. Current: {loan.Status}");

        loan.Disburse();

        for (int i = 1; i <= loan.TermMonths; i++)
        {
            var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(i));
            var repayment = Repayment.Create(
                request.TenantId,
                loan.Id,
                i,
                loan.GetMonthlyInstallment(),
                dueDate
            );
            _context.AddRepayment(repayment);
        }

        var auditLog = AuditLog.Create(
            request.TenantId,
            "Loan",
            loan.Id,
            "Disbursed",
            $"Disbursed via {request.Method} to account {request.AccountNumber}",
            "system"
        );
        _context.AddAuditLog(auditLog);

        await _context.SaveChangesAsync(ct);

        var result = new DisburseLoanResult(
            LoanId: loan.Id,
            Status: loan.Status.ToString(),
            DisbursementDate: loan.DisbursementDate!.Value,
            Amount: loan.PrincipalAmount,
            Message: $"Loan disbursed successfully via {request.Method}"
        );

        await _idempotencyService.StoreResultAsync(request.IdempotencyKey, JsonSerializer.Serialize(result));

        return result;
    }
}
