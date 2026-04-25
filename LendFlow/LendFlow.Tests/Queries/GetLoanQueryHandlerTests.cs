using LendFlow.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Queries.GetLoan;
using LendFlow.Domain.Entities;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Queries;

public class GetLoanQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsLoan_WhenFound()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetLoanQueryHandler(dbContext);

        var tenantId = Guid.NewGuid();
        var applicationId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();

        var loan = Loan.Create(
            tenantId,
            applicationId,
            applicantId,
            5000m,
            0.1m,
            12,
            new DateOnly(2025, 1, 1)
        );

        dbContext.AddLoan(loan);

        // Act
        var result = await handler.Handle(new GetLoanQuery(loan.Id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(loan.Id, result.Id);
        Assert.Equal(loan.TenantId, result.TenantId);
        Assert.Equal(loan.ApplicationId, result.ApplicationId);
        Assert.Equal(loan.ApplicantId, result.ApplicantId);
        Assert.Equal(loan.PrincipalAmount, result.PrincipalAmount);
        Assert.Equal(loan.InterestRate, result.InterestRate);
        Assert.Equal(loan.TermMonths, result.TermMonths);
        Assert.Equal(loan.RepaymentFrequency, result.RepaymentFrequency);
        Assert.Equal(loan.DisbursementDate, result.DisbursementDate);
        Assert.Equal(loan.MaturityDate, result.MaturityDate);
        Assert.Equal(loan.OutstandingBalance, result.OutstandingBalance);
        Assert.Equal(loan.Status.ToString(), result.Status);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenNotFound()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetLoanQueryHandler(dbContext);
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetLoanQuery(nonExistentId), CancellationToken.None));
    }
}
