using System;
using System.Collections.Generic;
using LendFlow.Domain.Common;
using LendFlow.Domain.Enums;
using LendFlow.Domain.Events;
using Stateless;

namespace LendFlow.Domain.Entities;

public class LoanApplication : BaseAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ApplicantId { get; private set; }
    public decimal RequestedAmount { get; private set; }
    public int RequestedTermMonths { get; private set; }
    public string Purpose { get; private set; } = string.Empty;
    public LoanApplicationStatus Status { get; private set; }
    public int? CreditScore { get; private set; }
    public string? RiskBand { get; private set; }
    public string? DecisionReason { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private StateMachine<LoanApplicationStatus, LoanApplicationTrigger>? _machine;

    private LoanApplication() 
    {
    }

    private StateMachine<LoanApplicationStatus, LoanApplicationTrigger> GetMachine()
    {
        if (_machine != null) return _machine;
        
        _machine = new StateMachine<LoanApplicationStatus, LoanApplicationTrigger>(
            () => Status, 
            s => Status = s);

        _machine.Configure(LoanApplicationStatus.Draft)
            .Permit(LoanApplicationTrigger.Submit, LoanApplicationStatus.Submitted);

        _machine.Configure(LoanApplicationStatus.Submitted)
            .Permit(LoanApplicationTrigger.Review, LoanApplicationStatus.UnderReview)
            .Permit(LoanApplicationTrigger.Approve, LoanApplicationStatus.Approved)
            .Permit(LoanApplicationTrigger.Reject, LoanApplicationStatus.Rejected)
            .Permit(LoanApplicationTrigger.Cancel, LoanApplicationStatus.Cancelled);

        _machine.Configure(LoanApplicationStatus.UnderReview)
            .Permit(LoanApplicationTrigger.Approve, LoanApplicationStatus.Approved)
            .Permit(LoanApplicationTrigger.Reject, LoanApplicationStatus.Rejected);

        return _machine;
    }

    public static LoanApplication Create(Guid tenantId, Guid applicantId, decimal requestedAmount, int requestedTermMonths, string purpose, string idempotencyKey)
    {
        var app = new LoanApplication
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicantId = applicantId,
            RequestedAmount = requestedAmount,
            RequestedTermMonths = requestedTermMonths,
            Purpose = purpose,
            Status = LoanApplicationStatus.Draft,
            IdempotencyKey = idempotencyKey
        };
        app._domainEvents.Add(new LoanApplicationCreatedEvent(app.Id));
        return app;
    }

    public void Submit()
    {
        GetMachine().Fire(LoanApplicationTrigger.Submit);
        _domainEvents.Add(new LoanApplicationSubmittedEvent(Id));
    }

    public void Review()
    {
        GetMachine().Fire(LoanApplicationTrigger.Review);
    }

    public void Approve(string reason)
    {
        DecisionReason = reason;
        GetMachine().Fire(LoanApplicationTrigger.Approve);
        _domainEvents.Add(new LoanApplicationApprovedEvent(Id));
    }

    public void Reject(string reason)
    {
        DecisionReason = reason;
        GetMachine().Fire(LoanApplicationTrigger.Reject);
        _domainEvents.Add(new LoanApplicationRejectedEvent(Id, reason));
    }

    public void Cancel()
    {
        GetMachine().Fire(LoanApplicationTrigger.Cancel);
    }

    public void SetAssessmentResult(int creditScore, string riskBand)
    {
        CreditScore = creditScore;
        RiskBand = riskBand;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
