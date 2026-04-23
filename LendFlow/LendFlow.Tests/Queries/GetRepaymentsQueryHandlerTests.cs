using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Queries.GetRepayments;
using LendFlow.Domain.Entities;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Queries;

public class GetRepaymentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsRepayments_OrderedByInstallmentNumber()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetRepaymentsQueryHandler(dbContext);

        var tenantId = Guid.NewGuid();
        var loanId = Guid.NewGuid();

        // Adding out of order
        var repayment3 = Repayment.Create(tenantId, loanId, 3, 500m, new DateOnly(2025, 3, 1));
        var repayment1 = Repayment.Create(tenantId, loanId, 1, 500m, new DateOnly(2025, 1, 1));
        var repayment2 = Repayment.Create(tenantId, loanId, 2, 500m, new DateOnly(2025, 2, 1));

        dbContext.AddRepayment(repayment3);
        dbContext.AddRepayment(repayment1);
        dbContext.AddRepayment(repayment2);

        // Act
        var result = await handler.Handle(new GetRepaymentsQuery(loanId), CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].InstallmentNumber);
        Assert.Equal(repayment1.Id, result[0].Id);
        Assert.Equal(2, result[1].InstallmentNumber);
        Assert.Equal(repayment2.Id, result[1].Id);
        Assert.Equal(3, result[2].InstallmentNumber);
        Assert.Equal(repayment3.Id, result[2].Id);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyList_WhenNoRepayments()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetRepaymentsQueryHandler(dbContext);

        // Act
        var result = await handler.Handle(new GetRepaymentsQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }
}
