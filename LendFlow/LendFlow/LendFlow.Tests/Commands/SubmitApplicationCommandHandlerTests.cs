using System;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Commands.SubmitApplication;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using LendFlow.Domain.Exceptions;
using LendFlow.Tests.Testing;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Commands;

public class SubmitApplicationCommandHandlerTests
{
    private readonly IDomainEventDispatcher _eventDispatcher = new FakeDomainEventDispatcher();

    [Fact]
    public async Task Handle_WhenApplicantExists_SubmitsApplication()
    {
        var idempotency = new FakeIdempotencyService();
        var dbContext = new FakeAppDbContext();
        var handler = new SubmitApplicationCommandHandler(idempotency, dbContext, _eventDispatcher);

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

        Assert.Single(dbContext.LoanApplications);
        var application = dbContext.LoanApplications[0];
        Assert.Equal(result.ApplicationId, application.Id);
        Assert.Equal(LoanApplicationStatus.Submitted, application.Status);
    }

    [Fact]
    public async Task Handle_WhenTenantMismatch_ThrowsNotFound()
    {
        var idempotency = new FakeIdempotencyService();
        var dbContext = new FakeAppDbContext();
        var handler = new SubmitApplicationCommandHandler(idempotency, dbContext, _eventDispatcher);

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

public class FakeDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(LendFlow.Domain.Events.IDomainEvent domainEvent, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task DispatchManyAsync(System.Collections.Generic.IEnumerable<LendFlow.Domain.Events.IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
