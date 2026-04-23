using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Queries.GetLoans;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Queries;

public class GetLoansQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllLoans_ForTenant()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetLoansQueryHandler(dbContext);

        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();

        var first = Loan.Create(tenantId, Guid.NewGuid(), applicantId, 5000m, 0.1m, 12, new DateOnly(2025, 1, 1));
        first.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10);

        var second = Loan.Create(tenantId, Guid.NewGuid(), applicantId, 10000m, 0.15m, 24, new DateOnly(2026, 1, 1));
        second.CreatedAt = DateTimeOffset.UtcNow;

        dbContext.AddLoan(first);
        dbContext.AddLoan(second);

        // Act
        var result = await handler.Handle(new GetLoansQuery(null, 1, 10), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        // By default, fake db context orders by CreatedAt descending
        Assert.Equal(second.Id, result.Items[0].Id);
        Assert.Equal(first.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task Handle_FiltersByStatus()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetLoansQueryHandler(dbContext);

        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();

        var pendingLoan = Loan.Create(tenantId, Guid.NewGuid(), applicantId, 5000m, 0.1m, 12, new DateOnly(2025, 1, 1));
        
        var activeLoan = Loan.Create(tenantId, Guid.NewGuid(), applicantId, 10000m, 0.15m, 24, new DateOnly(2026, 1, 1));
        activeLoan.Disburse();

        dbContext.AddLoan(pendingLoan);
        dbContext.AddLoan(activeLoan);

        // Act
        var result = await handler.Handle(new GetLoansQuery(LoanStatus.Active, 1, 10), CancellationToken.None);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(activeLoan.Id, result.Items.First().Id);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoLoans()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetLoansQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetLoansQuery(null, 1, 10), CancellationToken.None);

        // Assert
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }
}
