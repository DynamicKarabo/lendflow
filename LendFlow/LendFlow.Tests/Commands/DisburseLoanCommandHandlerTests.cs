using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Commands.DisburseLoan;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using LendFlow.Tests.Testing;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Commands;

public class DisburseLoanCommandHandlerTests
{
    [Fact]
    public async Task Handle_Idempotent_ReturnsStoredResultOnDuplicateKey()
    {
        var dbContext = new FakeAppDbContext();
        var idempotency = new FakeIdempotencyService();
        var handler = new DisburseLoanCommandHandler(dbContext, idempotency);

        var key = "disburse-1";
        await idempotency.StoreResultAsync(key, "{\"LoanId\":\"" + Guid.NewGuid() + "\",\"Status\":\"Active\",\"DisbursementDate\":\"2024-01-01T00:00:00Z\",\"Amount\":1000,\"Message\":\"Success\"}");

        var command = new DisburseLoanCommand(Guid.NewGuid(), Guid.NewGuid(), "EFT", "123456", "FNB", key);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Active", result.Status);
        Assert.Equal("Success", result.Message);
        Assert.Empty(dbContext.Loans); // Ensures the normal flow didn't happen
    }

    [Fact]
    public async Task Handle_LoanNotFound_ThrowsInvalidOperationException()
    {
        var dbContext = new FakeAppDbContext();
        var idempotency = new FakeIdempotencyService();
        var handler = new DisburseLoanCommandHandler(dbContext, idempotency);

        var command = new DisburseLoanCommand(Guid.NewGuid(), Guid.NewGuid(), "EFT", "123", "FNB", "key2");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task Handle_WrongStatus_ThrowsInvalidOperationException()
    {
        var dbContext = new FakeAppDbContext();
        var idempotency = new FakeIdempotencyService();
        var handler = new DisburseLoanCommandHandler(dbContext, idempotency);
        var tenantId = Guid.NewGuid();

        var loan = Loan.Create(tenantId, Guid.NewGuid(), Guid.NewGuid(), 1000m, 0.28m, 12, new DateOnly(2025, 1, 1));
        loan.Disburse(); // Now Status is Active
        dbContext.AddLoan(loan);

        var command = new DisburseLoanCommand(tenantId, loan.Id, "EFT", "123", "FNB", "key3");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("PendingDisbursement", ex.Message);
    }

    [Fact]
    public async Task Handle_SuccessfulDisbursement_UpdatesStatusCreatesRepaymentsAndAuditLog()
    {
        var dbContext = new FakeAppDbContext();
        var idempotency = new FakeIdempotencyService();
        var handler = new DisburseLoanCommandHandler(dbContext, idempotency);
        var tenantId = Guid.NewGuid();

        var loan = Loan.Create(tenantId, Guid.NewGuid(), Guid.NewGuid(), 1000m, 0.28m, 12, new DateOnly(2025, 1, 1));
        dbContext.AddLoan(loan);

        var command = new DisburseLoanCommand(tenantId, loan.Id, "EFT", "123456", "FNB", "key4");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(LoanStatus.Active, loan.Status);
        Assert.NotNull(loan.DisbursementDate);
        
        Assert.Equal(12, dbContext.Repayments.Count);
        Assert.All(dbContext.Repayments, r => Assert.Equal(loan.Id, r.LoanId));
        
        Assert.Single(dbContext.AuditLogs);
        var log = dbContext.AuditLogs.Single();
        Assert.Equal(loan.Id, log.EntityId);
        Assert.Equal("Disbursed", log.Action);
    }
}
