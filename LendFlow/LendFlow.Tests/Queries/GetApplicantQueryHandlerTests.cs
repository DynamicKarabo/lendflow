using System;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Queries.GetApplicant;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Exceptions;
using LendFlow.Tests.Testing;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Queries;

public class GetApplicantQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsApplicant_WhenFound()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetApplicantQueryHandler(dbContext);
        
        var dob = new DateOnly(1990, 1, 15);
        var saId = TestData.CreateValidSaId(dob);
        
        var applicant = Applicant.Create(
            Guid.NewGuid(),
            "John",
            "Doe",
            saId,
            "0821234567",
            "john.doe@example.com",
            dob,
            "Employed",
            50000m,
            20000m
        );
        
        dbContext.AddApplicant(applicant);

        // Act
        var result = await handler.Handle(new GetApplicantQuery(applicant.Id), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(applicant.Id, result.Id);
        Assert.Equal(applicant.TenantId, result.TenantId);
        Assert.Equal(applicant.FirstName, result.FirstName);
        Assert.Equal(applicant.LastName, result.LastName);
        Assert.Equal(applicant.Email, result.Email);
    }

    [Fact]
    public async Task Handle_ThrowsNotFoundException_WhenNotFound()
    {
        // Arrange
        var dbContext = new FakeAppDbContext();
        var handler = new GetApplicantQueryHandler(dbContext);
        var nonExistentId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => 
            handler.Handle(new GetApplicantQuery(nonExistentId), CancellationToken.None));
    }
}
