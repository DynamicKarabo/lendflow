using System;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Commands.SubmitApplication;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using LendFlow.Domain.Exceptions;
using LendFlow.Tests.Testing;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Commands;

public class SubmitApplicationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenApplicantExists_SubmitsApplication()
    {
        var idempotency = new FakeIdempotencyService();
        var dbContext = new FakeAppDbContext();
        var handler = new SubmitApplicationCommandHandler(idempotency, dbContext);

        var tenantId = Guid.NewGuid();
        var dob = new DateOnly(1992, 5, 20);
        var applicant = Applicant.Create(
            tenantId,
            "Jane",
            "Smith",
            TestData.CreateValidSaId(dob),
            "0712345678",
            "jane.smith@example.com",
            dob,
            "Employed",
            15000m,
            4000m
        );
        dbContext.AddApplicant(applicant);

        var command = new SubmitApplicationCommand(
            TenantId: tenantId,
            ApplicantId: applicant.Id,
            RequestedAmount: 1000m,
            RequestedTermMonths: 3,
            Purpose: "working_capital",
            IdempotencyKey: "app-key-1"
        );

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(1, dbContext.LoanApplications.Count);
        var application = dbContext.LoanApplications[0];
        Assert.Equal(result.ApplicationId, application.Id);
        Assert.Equal(LoanApplicationStatus.Submitted, application.Status);
    }

    [Fact]
    public async Task Handle_WhenTenantMismatch_ThrowsNotFound()
    {
        var idempotency = new FakeIdempotencyService();
        var dbContext = new FakeAppDbContext();
        var handler = new SubmitApplicationCommandHandler(idempotency, dbContext);

        var dob = new DateOnly(1991, 3, 10);
        var applicant = Applicant.Create(
            Guid.NewGuid(),
            "Zola",
            "Nkosi",
            TestData.CreateValidSaId(dob),
            "0712345678",
            "zola.nkosi@example.com",
            dob,
            "Employed",
            12000m,
            3500m
        );
        dbContext.AddApplicant(applicant);

        var command = new SubmitApplicationCommand(
            TenantId: Guid.NewGuid(),
            ApplicantId: applicant.Id,
            RequestedAmount: 1000m,
            RequestedTermMonths: 3,
            Purpose: "working_capital",
            IdempotencyKey: "app-key-2"
        );

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }
}
