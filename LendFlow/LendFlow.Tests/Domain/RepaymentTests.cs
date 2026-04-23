using System;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using Xunit;

namespace LendFlow.Tests.Domain;

public class RepaymentTests
{
    private Repayment CreateDefaultRepayment()
    {
        return Repayment.Create(
            Guid.NewGuid(), Guid.NewGuid(), 1, 500m, new DateOnly(2023, 12, 1)
        );
    }

    [Fact]
    public void Create_SetsStatusToScheduled()
    {
        var repayment = CreateDefaultRepayment();
        Assert.Equal(RepaymentStatus.Scheduled, repayment.Status);
    }

    [Fact]
    public void RecordPayment_MovesToPaid()
    {
        var repayment = CreateDefaultRepayment();
        repayment.RecordPayment(500m, "REF123");
        
        Assert.Equal(RepaymentStatus.Paid, repayment.Status);
        Assert.Equal(500m, repayment.AmountPaid);
        Assert.Equal("REF123", repayment.PaymentReference);
        Assert.NotNull(repayment.PaidDate);
    }

    [Fact]
    public void AmountPaid_CanExceedAmountDue()
    {
        var repayment = CreateDefaultRepayment();
        repayment.RecordPayment(600m, "REF124");
        
        Assert.Equal(RepaymentStatus.Paid, repayment.Status);
        Assert.Equal(600m, repayment.AmountPaid);
        Assert.Equal(500m, repayment.AmountDue);
    }
}
