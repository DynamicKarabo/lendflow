using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Commands.MakeDecision;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using LendFlow.Tests.Testing;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Commands;

public class MakeDecisionCommandHandlerTests
{
    private readonly FakeDomainEventDispatcher _eventDispatcher = new FakeDomainEventDispatcher();

    [Fact]
    public async Task Handle_ApplicationNotFound_ThrowsInvalidOperationException()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new MakeDecisionCommandHandler(dbContext, _eventDispatcher);

        var command = new MakeDecisionCommand(Guid.NewGuid(), Guid.NewGuid(), "Approved", "Looks good");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task Handle_WrongStatus_ThrowsInvalidOperationException()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new MakeDecisionCommandHandler(dbContext, _eventDispatcher);
        var tenantId = Guid.NewGuid();

        var application = LoanApplication.Create(tenantId, Guid.NewGuid(), 1000m, 12, "Test", "key1");
        // Status is Draft
        dbContext.AddLoanApplication(application);

        var command = new MakeDecisionCommand(tenantId, application.Id, "Approved", "Looks good");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("under review", ex.Message);
    }

    [Fact]
    public async Task Handle_InvalidDecision_ThrowsInvalidOperationException()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new MakeDecisionCommandHandler(dbContext, _eventDispatcher);
        var tenantId = Guid.NewGuid();

        var application = LoanApplication.Create(tenantId, Guid.NewGuid(), 1000m, 12, "Test", "key2");
        application.Submit();
        application.Review();
        dbContext.AddLoanApplication(application);

        var command = new MakeDecisionCommand(tenantId, application.Id, "Maybe", "Not sure");
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        
        Assert.Contains("must be 'approved' or 'rejected'", ex.Message);
    }

    [Fact]
    public async Task Handle_Approve_CreatesLoanAndSetsStatusApproved()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new MakeDecisionCommandHandler(dbContext, _eventDispatcher);
        var tenantId = Guid.NewGuid();

        var application = LoanApplication.Create(tenantId, Guid.NewGuid(), 1000m, 12, "Test", "key3");
        application.Submit();
        application.Review();
        dbContext.AddLoanApplication(application);

        var command = new MakeDecisionCommand(tenantId, application.Id, "Approved", "Looks great");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Approved", result.Decision);
        Assert.Equal(LoanApplicationStatus.Approved, application.Status);
        Assert.Equal("Looks great", application.DecisionReason);
        
        Assert.Single(dbContext.Loans);
        var loan = dbContext.Loans.Single();
        Assert.Equal(application.Id, loan.ApplicationId);
        
        Assert.Equal(loan.Id, result.LoanId);
        
        // Ensure repayment schedule was generated on the loan, which is part of MakeDecision in this app
        Assert.Equal(12, loan.Repayments.Count);
    }

    [Fact]
    public async Task Handle_Reject_SetsStatusRejected()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new MakeDecisionCommandHandler(dbContext, _eventDispatcher);
        var tenantId = Guid.NewGuid();

        var application = LoanApplication.Create(tenantId, Guid.NewGuid(), 1000m, 12, "Test", "key4");
        application.Submit();
        application.Review();
        dbContext.AddLoanApplication(application);

        var command = new MakeDecisionCommand(tenantId, application.Id, "Rejected", "Too risky");
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Rejected", result.Decision);
        Assert.Equal(LoanApplicationStatus.Rejected, application.Status);
        Assert.Equal("Too risky", application.DecisionReason);
        
        Assert.Empty(dbContext.Loans);
        Assert.Null(result.LoanId);
    }
}
