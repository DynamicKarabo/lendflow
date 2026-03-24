using System;
using System.Collections.Generic;
using LendFlow.Domain.Common;
using LendFlow.Domain.Enums;
using Stateless;

namespace LendFlow.Domain.Entities;

public class Loan : BaseAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid ApplicantId { get; private set; }
    public decimal PrincipalAmount { get; private set; }
    public decimal InterestRate { get; private set; }
    public int TermMonths { get; private set; }
    public string RepaymentFrequency { get; private set; } = "Monthly";
    public DateTimeOffset? DisbursementDate { get; private set; }
    public DateOnly MaturityDate { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public LoanStatus Status { get; private set; }

    private readonly List<Repayment> _repayments = new();
    public IReadOnlyCollection<Repayment> Repayments => _repayments.AsReadOnly();

    public Applicant? Applicant { get; private set; }

    private StateMachine<LoanStatus, LoanTrigger>? _machine;

    private Loan() { }

    private StateMachine<LoanStatus, LoanTrigger> GetMachine()
    {
        if (_machine != null) return _machine;
        
        _machine = new StateMachine<LoanStatus, LoanTrigger>(
            () => Status, 
            s => Status = s);

        _machine.Configure(LoanStatus.PendingDisbursement)
            .Permit(LoanTrigger.Disburse, LoanStatus.Active);

        _machine.Configure(LoanStatus.Active)
            .Permit(LoanTrigger.Settle, LoanStatus.Settled)
            .Permit(LoanTrigger.Default, LoanStatus.Defaulted)
            .Permit(LoanTrigger.WriteOff, LoanStatus.WrittenOff);

        return _machine;
    }

    public static Loan Create(
        Guid tenantId,
        Guid applicationId,
        Guid applicantId,
        decimal principalAmount,
        decimal interestRate,
        int termMonths,
        DateOnly maturityDate)
    {
        return new Loan
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicationId = applicationId,
            ApplicantId = applicantId,
            PrincipalAmount = principalAmount,
            InterestRate = interestRate,
            TermMonths = termMonths,
            MaturityDate = maturityDate,
            OutstandingBalance = CalculateTotalPayable(principalAmount, interestRate, termMonths),
            Status = LoanStatus.PendingDisbursement
        };
    }

    private static decimal CalculateTotalPayable(decimal principal, decimal annualRate, int months)
    {
        var monthlyRate = annualRate / 12;
        if (monthlyRate == 0) return principal;
        
        var rate = (double)monthlyRate;
        var monthlyPayment = (decimal)(rate * Math.Pow(1 + rate, months)) / 
                            ((decimal)Math.Pow(1 + rate, months) - 1);
        return Math.Round(principal * monthlyPayment * months, 2);
    }

    public decimal GetMonthlyInstallment()
    {
        var monthlyRate = InterestRate / 12;
        if (monthlyRate == 0) return PrincipalAmount / TermMonths;
        
        var rate = (double)monthlyRate;
        var term = TermMonths;
        var monthlyPayment = (decimal)(rate * Math.Pow(1 + rate, term)) / 
                            ((decimal)Math.Pow(1 + rate, term) - 1);
        return Math.Round(PrincipalAmount * monthlyPayment, 2);
    }

    public void Disburse()
    {
        GetMachine().Fire(LoanTrigger.Disburse);
        DisbursementDate = DateTimeOffset.UtcNow;
    }

    public void RecordRepayment(decimal amount)
    {
        if (Status != LoanStatus.Active)
            throw new InvalidOperationException("Can only record repayments for active loans.");

        OutstandingBalance -= amount;
        if (OutstandingBalance <= 0)
        {
            OutstandingBalance = 0;
            GetMachine().Fire(LoanTrigger.Settle);
        }
    }

    public void MarkDefaulted()
    {
        GetMachine().Fire(LoanTrigger.Default);
    }

    public void MarkWrittenOff()
    {
        GetMachine().Fire(LoanTrigger.WriteOff);
    }

    public void AddRepayment(Repayment repayment)
    {
        _repayments.Add(repayment);
    }
}
