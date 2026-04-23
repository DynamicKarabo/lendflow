using System;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using Xunit;

namespace LendFlow.Tests.Domain;

public class LoanApplicationTests
{
    private LoanApplication CreateDefaultApplication()
    {
        return LoanApplication.Create(
            Guid.NewGuid(), Guid.NewGuid(), 5000m, 12, "Home repair", Guid.NewGuid().ToString()
        );
    }

    [Fact]
    public void Create_SetsStatusToDraft()
    {
        var app = CreateDefaultApplication();
        Assert.Equal(LoanApplicationStatus.Draft, app.Status);
    }

    [Fact]
    public void Submit_MovesToSubmitted()
    {
        var app = CreateDefaultApplication();
        app.Submit();
        Assert.Equal(LoanApplicationStatus.Submitted, app.Status);
    }

    [Fact]
    public void Approve_MovesToApproved()
    {
        var app = CreateDefaultApplication();
        app.Submit();
        app.Review();
        app.Approve("Looks good");
        Assert.Equal(LoanApplicationStatus.Approved, app.Status);
        Assert.Equal("Looks good", app.DecisionReason);
    }

    [Fact]
    public void Reject_MovesToRejected()
    {
        var app = CreateDefaultApplication();
        app.Submit();
        app.Reject("Bad credit");
        Assert.Equal(LoanApplicationStatus.Rejected, app.Status);
        Assert.Equal("Bad credit", app.DecisionReason);
    }

    [Fact]
    public void Review_MovesToUnderReview()
    {
        var app = CreateDefaultApplication();
        app.Submit();
        app.Review();
        Assert.Equal(LoanApplicationStatus.UnderReview, app.Status);
    }

    [Fact]
    public void Cancel_FromSubmitted_Succeeds()
    {
        var app = CreateDefaultApplication();
        app.Submit();
        app.Cancel();
        Assert.Equal(LoanApplicationStatus.Cancelled, app.Status);
    }

    [Fact]
    public void Cancel_FromApproved_ThrowsInvalidOperationException()
    {
        var app = CreateDefaultApplication();
        app.Submit();
        app.Approve("Approved automatically");
        
        Assert.Throws<InvalidOperationException>(() => app.Cancel());
    }

    [Fact]
    public void SetAssessmentResult_SetsScoreAndRiskBand()
    {
        var app = CreateDefaultApplication();
        app.SetAssessmentResult(700, "Low");
        
        Assert.Equal(700, app.CreditScore);
        Assert.Equal("Low", app.RiskBand);
    }
}
