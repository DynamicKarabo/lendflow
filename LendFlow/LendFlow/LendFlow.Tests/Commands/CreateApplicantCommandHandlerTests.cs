using System;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Commands.CreateApplicant;
using LendFlow.Tests.Testing;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Commands;

public class CreateApplicantCommandHandlerTests
{
    [Fact]
    public async Task Handle_IsIdempotent_ReturnsStoredResult()
    {
        var idempotency = new FakeIdempotencyService();
        var dbContext = new FakeAppDbContext();
        var handler = new CreateApplicantCommandHandler(idempotency, dbContext);

        var dob = new DateOnly(1990, 1, 15);
        var command = new CreateApplicantCommand(
            TenantId: Guid.NewGuid(),
            FirstName: "John",
            LastName: "Doe",
            IdNumber: TestData.CreateValidSaId(dob),
            PhoneNumber: "0712345678",
            Email: "john.doe@example.com",
            DateOfBirth: dob,
            EmploymentStatus: "Employed",
            MonthlyIncome: 10000m,
            MonthlyExpenses: 3000m,
            IdempotencyKey: "key-1"
        );

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command, CancellationToken.None);

        Assert.Equal(first.ApplicantId, second.ApplicantId);
        Assert.Single(dbContext.Applicants);
    }
}
