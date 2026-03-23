using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.Common.Models;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;

namespace LendFlow.Tests.Testing.Fakes;

public class FakeAppDbContext : IAppDbContext
{
    public List<Applicant> Applicants { get; } = new();
    public List<LoanApplication> LoanApplications { get; } = new();
    public List<Loan> Loans { get; } = new();
    public List<Repayment> Repayments { get; } = new();
    public List<CreditAssessment> CreditAssessments { get; } = new();
    public List<AuditLog> AuditLogs { get; } = new();

    public Task<Applicant?> GetApplicantAsync(Guid id, CancellationToken ct)
    {
        return Task.FromResult(Applicants.FirstOrDefault(a => a.Id == id));
    }

    public Task<LoanApplication?> GetLoanApplicationAsync(Guid id, CancellationToken ct)
    {
        return Task.FromResult(LoanApplications.FirstOrDefault(a => a.Id == id));
    }

    public Task<Loan?> GetLoanAsync(Guid id, CancellationToken ct)
    {
        return Task.FromResult(Loans.FirstOrDefault(l => l.Id == id));
    }

    public Task<PagedResult<LoanApplication>> GetLoanApplicationsAsync(LoanApplicationStatus? status, int pageNumber, int pageSize, CancellationToken ct)
    {
        IEnumerable<LoanApplication> query = LoanApplications;
        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var ordered = query.OrderByDescending(a => a.CreatedAt);
        var total = ordered.Count();
        var items = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<LoanApplication>(items, total, pageNumber, pageSize));
    }

    public Task<PagedResult<Loan>> GetLoansAsync(LoanStatus? status, int pageNumber, int pageSize, CancellationToken ct)
    {
        IEnumerable<Loan> query = Loans;
        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        var ordered = query.OrderByDescending(l => l.CreatedAt);
        var total = ordered.Count();
        var items = ordered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<Loan>(items, total, pageNumber, pageSize));
    }

    public Task<List<Repayment>> GetRepaymentsByLoanIdAsync(Guid loanId, CancellationToken ct)
    {
        return Task.FromResult(Repayments.Where(r => r.LoanId == loanId).OrderBy(r => r.InstallmentNumber).ToList());
    }

    public void AddApplicant(Applicant applicant)
    {
        if (applicant.CreatedAt == default)
        {
            applicant.CreatedAt = DateTimeOffset.UtcNow;
        }

        Applicants.Add(applicant);
    }

    public void AddLoanApplication(LoanApplication application)
    {
        if (application.CreatedAt == default)
        {
            application.CreatedAt = DateTimeOffset.UtcNow;
        }

        LoanApplications.Add(application);
    }

    public void AddLoan(Loan loan)
    {
        if (loan.CreatedAt == default)
        {
            loan.CreatedAt = DateTimeOffset.UtcNow;
        }

        Loans.Add(loan);
    }

    public void AddRepayment(Repayment repayment)
    {
        if (repayment.CreatedAt == default)
        {
            repayment.CreatedAt = DateTimeOffset.UtcNow;
        }

        Repayments.Add(repayment);
    }

    public void AddCreditAssessment(CreditAssessment assessment)
    {
        if (assessment.CreatedAt == default)
        {
            assessment.CreatedAt = DateTimeOffset.UtcNow;
        }

        CreditAssessments.Add(assessment);
    }

    public void AddAuditLog(AuditLog auditLog)
    {
        AuditLogs.Add(auditLog);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return Task.FromResult(1);
    }
}
