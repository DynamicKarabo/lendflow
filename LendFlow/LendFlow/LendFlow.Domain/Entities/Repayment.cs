using System;
using LendFlow.Domain.Common;
using LendFlow.Domain.Enums;

namespace LendFlow.Domain.Entities;

public class Repayment : BaseAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid LoanId { get; private set; }
    public int InstallmentNumber { get; private set; }
    public decimal AmountDue { get; private set; }
    public decimal? AmountPaid { get; private set; }
    public DateOnly DueDate { get; private set; }
    public DateTimeOffset? PaidDate { get; private set; }
    public RepaymentStatus Status { get; private set; }
    public string? PaymentReference { get; private set; }

    public Loan? Loan { get; private set; }

    private Repayment() { }

    public static Repayment Create(
        Guid tenantId,
        Guid loanId,
        int installmentNumber,
        decimal amountDue,
        DateOnly dueDate)
    {
        return new Repayment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LoanId = loanId,
            InstallmentNumber = installmentNumber,
            AmountDue = amountDue,
            DueDate = dueDate,
            Status = RepaymentStatus.Scheduled
        };
    }

    public void RecordPayment(decimal amount, string paymentReference)
    {
        if (Status == RepaymentStatus.Paid)
            throw new InvalidOperationException("Repayment already paid.");

        AmountPaid = amount;
        PaidDate = DateTimeOffset.UtcNow;
        PaymentReference = paymentReference;
        Status = RepaymentStatus.Paid;
    }

    public void MarkLate()
    {
        if (Status == RepaymentStatus.Scheduled)
            Status = RepaymentStatus.Late;
    }

    public void MarkMissed()
    {
        Status = RepaymentStatus.Missed;
    }

    public void UpdateStatus(RepaymentStatus newStatus)
    {
        Status = newStatus;
    }

    public bool IsOverdue => Status == RepaymentStatus.Scheduled && DueDate < DateOnly.FromDateTime(DateTime.UtcNow);
}
