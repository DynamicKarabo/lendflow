using System;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Queries.GetLoanApplication;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Exceptions;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Queries;

public class GetLoanApplicationQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsApplication_WhenFound()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetLoanApplicationQueryHandler(dbContext);

        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();

        var application = LoanApplication.Create(
            tenantId,
            applicantId,
            1000m,
            3,
            "working_capital",
            "idempotency-key-1"
        );

        dbContext.AddLoanApplication(application);

        // Act
        var result = await handler.Handle(new GetLoanApplicationQuery(application.Id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(application.Id, result.Id);
        Assert.Equal(application.TenantId, result.TenantId);
        Assert.Equal(application.ApplicantId, result.ApplicantId);
        Assert.Equal(application.RequestedAmount, result.RequestedAmount);
        Assert.Equal(application.RequestedTermMonths, result.RequestedTermMonths);
        Assert.Equal(application.Purpose, result.Purpose);
        Assert.Equal(application.Status, result.Status);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenNotFound()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetLoanApplicationQueryHandler(dbContext);
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetLoanApplicationQuery(nonExistentId), CancellationToken.None));
    }
}
