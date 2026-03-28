using System;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Queries.GetLoanApplications;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Queries;

public class GetLoanApplicationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedResults()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new GetLoanApplicationsQueryHandler(dbContext);

        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();

        var first = LoanApplication.Create(tenantId, applicantId, 1000m, 3, "working_capital", "id-1");
        first.Submit();
        first.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10);

        var second = LoanApplication.Create(tenantId, applicantId, 2000m, 6, "education", "id-2");
        second.Submit();
        second.CreatedAt = DateTimeOffset.UtcNow;

        dbContext.AddLoanApplication(first);
        dbContext.AddLoanApplication(second);

        var result = await handler.Handle(new GetLoanApplicationsQuery(null, 1, 1), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(second.Id, result.Items.First().Id);
    }

    [Fact]
    public async Task Handle_FiltersByStatus()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new GetLoanApplicationsQueryHandler(dbContext);

        var tenantId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();

        var submitted = LoanApplication.Create(tenantId, applicantId, 1000m, 3, "working_capital", "id-1");
        submitted.Submit();

        var rejected = LoanApplication.Create(tenantId, applicantId, 1000m, 3, "working_capital", "id-2");
        rejected.Submit();
        rejected.Reject("low score");

        dbContext.AddLoanApplication(submitted);
        dbContext.AddLoanApplication(rejected);

        var result = await handler.Handle(new GetLoanApplicationsQuery(LoanApplicationStatus.Submitted, 1, 10), CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(submitted.Id, result.Items.First().Id);
    }
}
