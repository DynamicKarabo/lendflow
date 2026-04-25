using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Commands.AssessCredit;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.CreditScoring;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using LendFlow.Tests.Testing;
using LendFlow.Tests.Testing.Fakes;
using Xunit;

namespace LendFlow.Tests.Commands;

public class AssessCreditCommandHandlerTests
{
    private readonly CreditAssessmentService _assessmentService;
    private readonly DecisionEngine _decisionEngine;

    public AssessCreditCommandHandlerTests()
    {
        var factors = new List<ICreditScoringFactor>
        {
            new EmploymentStatusFactor(),
            new IncomeStabilityFactor(),
            new DebtToIncomeFactor(),
            new LoanAmountFactor()
        };
        _assessmentService = new CreditAssessmentService(factors);
        _decisionEngine = new DecisionEngine();
    }

    [Fact]
    public async Task Handle_ApplicationNotFound_ThrowsInvalidOperationException()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new AssessCreditCommandHandler(dbContext, _assessmentService, _decisionEngine);
        var command = new AssessCreditCommand(Guid.NewGuid(), Guid.NewGuid());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task Handle_ApplicantNotFound_ThrowsInvalidOperationException()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new AssessCreditCommandHandler(dbContext, _assessmentService, _decisionEngine);
        var tenantId = Guid.NewGuid();

        var application = LoanApplication.Create(tenantId, Guid.NewGuid(), 1000m, 12, "Test", "key1");
        dbContext.AddLoanApplication(application);

        var command = new AssessCreditCommand(tenantId, application.Id);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task Handle_HighScore_ApprovesApplicationAndCreatesLoan()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new AssessCreditCommandHandler(dbContext, _assessmentService, _decisionEngine);
        var tenantId = Guid.NewGuid();
        var dob = new DateOnly(1990, 1, 15);

        var applicant = Applicant.Create(tenantId, "Test", "User", TestData.CreateValidSaId(dob), "0123456789", "test@test.com", dob, "Employed", 50000m, 1000m);
        dbContext.AddApplicant(applicant);

        var application = LoanApplication.Create(tenantId, applicant.Id, 1000m, 12, "Test", "key1");
        application.Submit();
        application.Review();
        dbContext.AddLoanApplication(application);

        var command = new AssessCreditCommand(tenantId, application.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Approved", result.Decision);
        Assert.Equal(LoanApplicationStatus.Approved, application.Status);

        Assert.Single(dbContext.Loans);
        var loan = dbContext.Loans.Single();
        Assert.Equal(application.Id, loan.ApplicationId);

        Assert.Single(dbContext.CreditAssessments);
    }

    [Fact]
    public async Task Handle_MediumScore_SetsStatusToUnderReview()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new AssessCreditCommandHandler(dbContext, _assessmentService, _decisionEngine);
        var tenantId = Guid.NewGuid();
        var dob = new DateOnly(1990, 1, 15);

        var applicant = Applicant.Create(tenantId, "Test", "User", TestData.CreateValidSaId(dob), "0123456789", "test@test.com", dob, "SelfEmployed", 15000m, 4000m);
        dbContext.AddApplicant(applicant);

        var application = LoanApplication.Create(tenantId, applicant.Id, 50000m, 12, "Test", "key1");
        application.Submit();
        dbContext.AddLoanApplication(application);

        var command = new AssessCreditCommand(tenantId, application.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("UnderReview", result.Decision);
        Assert.Equal(LoanApplicationStatus.UnderReview, application.Status);
        Assert.Empty(dbContext.Loans);

        Assert.Single(dbContext.CreditAssessments);
    }

    [Fact]
    public async Task Handle_LowScore_RejectsApplication()
    {
        var dbContext = new FakeAppDbContext();
        var handler = new AssessCreditCommandHandler(dbContext, _assessmentService, _decisionEngine);
        var tenantId = Guid.NewGuid();
        var dob = new DateOnly(1990, 1, 15);

        var applicant = Applicant.Create(tenantId, "Test", "User", TestData.CreateValidSaId(dob), "0123456789", "test@test.com", dob, "Unemployed", 3000m, 2000m);
        dbContext.AddApplicant(applicant);

        var application = LoanApplication.Create(tenantId, applicant.Id, 10000m, 12, "Test", "key1");
        application.Submit();
        dbContext.AddLoanApplication(application);

        var command = new AssessCreditCommand(tenantId, application.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Rejected", result.Decision);
        Assert.Equal(LoanApplicationStatus.Rejected, application.Status);
        Assert.Empty(dbContext.Loans);

        Assert.Single(dbContext.CreditAssessments);
    }
}
