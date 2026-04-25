using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Commands.RecordRepayment;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using LendFlow.Tests.Testing;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Commands;

public class RecordRepaymentCommandHandlerTests
{
    [Fact]
    public async Task Handle_Idempotent_ReturnsStoredResultOnDuplicateKey()
    {
        var dbContext = new FakeAppDbContext();
        var idempotency = new FakeIdempotencyService();
        var handler = new RecordRepaymentCommandHandler(dbContext, idempotency);

        var key = "repay-1";
        await idempotency.StoreResultAsync(key, "{\"LoanId\":\"" + Guid.NewGuid() + "\",\"AmountPaid\":100,\"OutstandingBalance\":900,\"LoanStatus\":\"Active\",\"RepaymentsRemaining\":11}");

        var command = new RecordRepaymentCommand(Guid.NewGuid(), Guid.NewGuid(), 100m, "REF123", key);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(100m, result.AmountPaid);
        Assert.Equal(900m, result.OutstandingBalance);
        Assert.Equal("Active", result.LoanStatus);
        Assert.Equal(11, result.RepaymentsRemaining);
    }

    [Fact]
    public async Task Handle_LoanNotFound_ThrowsInvalidOperationException()
    {
        var dbContext = new FakeAppDbContext();
        var idempotency = new FakeIdempotencyService();
        var handler = new RecordRepaymentCommandHandler(dbContext, idempotency);

        var command = new RecordRepaymentCommand(Guid.NewGuid(), Guid.NewGuid(), 100m, "REF123", "key2");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task Handle_InactiveLoan_ThrowsInvalidOperationException()
    {
        var dbContext = new FakeAppDbContext();
        var idempotency = new FakeIdempotencyService();
        var handler = new RecordRepaymentCommandHandler(dbContext, idempotency);
        var tenantId = Guid.NewGuid();

        var loan = Loan.Create(tenantId, Guid.NewGuid(), Guid.NewGuid(), 1000m, 0.28m, 12, new DateOnly(2025, 1, 1));
        // Status is PendingDisbursement
        dbContext.AddLoan(loan);

        var command = new RecordRepaymentCommand(tenantId, loan.Id, 100m, "REF123", "key3");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));

        Assert.Contains("must be Active", ex.Message);
    }

    [Fact]
    public async Task Handle_AmountBelowDue_ThrowsInvalidOperationException()
    {
        var dbContext = new FakeAppDbContext();
        var idempotency = new FakeIdempotencyService();
        var handler = new RecordRepaymentCommandHandler(dbContext, idempotency);
        var tenantId = Guid.NewGuid();

        var loan = Loan.Create(tenantId, Guid.NewGuid(), Guid.NewGuid(), 1000m, 0.28m, 12, new DateOnly(2025, 1, 1));
        loan.Disburse();
        dbContext.AddLoan(loan);

        var repayment = Repayment.Create(tenantId, loan.Id, 1, 100m, new DateOnly(2024, 2, 1));
        dbContext.AddRepayment(repayment);

        var command = new RecordRepaymentCommand(tenantId, loan.Id, 50m, "REF123", "key4");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));

        Assert.Contains("Payment must be at least", ex.Message);
    }

    [Fact]
    public async Task Handle_SuccessfulRepayment_UpdatesStatusDecreasesBalanceCreatesAuditLog()
    {
        var dbContext = new FakeAppDbContext();
        var idempotency = new FakeIdempotencyService();
        var handler = new RecordRepaymentCommandHandler(dbContext, idempotency);
        var tenantId = Guid.NewGuid();

        var loan = Loan.Create(tenantId, Guid.NewGuid(), Guid.NewGuid(), 1000m, 0.28m, 12, new DateOnly(2025, 1, 1));
        loan.Disburse();
        dbContext.AddLoan(loan);

        var repayment1 = Repayment.Create(tenantId, loan.Id, 1, 100m, new DateOnly(2024, 2, 1));
        var repayment2 = Repayment.Create(tenantId, loan.Id, 2, 100m, new DateOnly(2024, 3, 1));
        dbContext.AddRepayment(repayment1);
        dbContext.AddRepayment(repayment2);

        var initialBalance = loan.OutstandingBalance;

        var command = new RecordRepaymentCommand(tenantId, loan.Id, 100m, "REF123", "key5");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(RepaymentStatus.Paid, repayment1.Status);
        Assert.Equal(100m, repayment1.AmountPaid);
        Assert.Equal("REF123", repayment1.PaymentReference);

        Assert.Equal(initialBalance - 100m, loan.OutstandingBalance);

        Assert.Single(dbContext.AuditLogs);
        var log = dbContext.AuditLogs.Single();
        Assert.Equal(repayment1.Id, log.EntityId);
        Assert.Equal("Paid", log.Action);

        Assert.Equal(0, result.RepaymentsRemaining);
    }

    [Fact]
    public async Task Handle_LastRepayment_SettlesTheLoan()
    {
        var dbContext = new FakeAppDbContext();
        var idempotency = new FakeIdempotencyService();
        var handler = new RecordRepaymentCommandHandler(dbContext, idempotency);
        var tenantId = Guid.NewGuid();

        var loan = Loan.Create(tenantId, Guid.NewGuid(), Guid.NewGuid(), 100m, 0.0m, 1, new DateOnly(2024, 2, 1));
        loan.Disburse();
        dbContext.AddLoan(loan);

        var repayment = Repayment.Create(tenantId, loan.Id, 1, 100m, new DateOnly(2024, 2, 1));
        dbContext.AddRepayment(repayment);

        var command = new RecordRepaymentCommand(tenantId, loan.Id, 100m, "REF123", "key6");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(0m, loan.OutstandingBalance);
        Assert.Equal(LoanStatus.Settled, loan.Status);

        Assert.Equal("Settled", result.LoanStatus);
        Assert.Equal(0, result.RepaymentsRemaining);
    }
}
