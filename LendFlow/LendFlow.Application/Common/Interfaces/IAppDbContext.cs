using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Models;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LendFlow.Application.Common.Interfaces;

public interface IAppDbContext
{
    IQueryable<LoanApplication> LoanApplications { get; }
    IQueryable<Loan> Loans { get; }
    IQueryable<Repayment> Repayments { get; }
    IQueryable<Applicant> Applicants { get; }

    Task<Applicant?> GetApplicantAsync(Guid id, CancellationToken ct);
    Task<LoanApplication?> GetLoanApplicationAsync(Guid id, CancellationToken ct);
    Task<Loan?> GetLoanAsync(Guid id, CancellationToken ct);
    Task<PagedResult<LoanApplication>> GetLoanApplicationsAsync(LoanApplicationStatus? status, int pageNumber, int pageSize, CancellationToken ct);
    Task<PagedResult<Loan>> GetLoansAsync(LoanStatus? status, int pageNumber, int pageSize, CancellationToken ct);
    Task<List<Repayment>> GetRepaymentsByLoanIdAsync(Guid loanId, CancellationToken ct);
    Task<List<Repayment>> GetUpcomingRepaymentsAsync(DateOnly reminderDate, CancellationToken ct);
    Task<List<Repayment>> GetLateRepaymentsAsync(DateOnly today, CancellationToken ct);
    Task<List<LoanApplication>> GetOldRejectedApplicationsAsync(DateTime cutoffDate, CancellationToken ct);
    Task<List<Loan>> GetOldSettledLoansAsync(DateTime cutoffDate, CancellationToken ct);
    void AddApplicant(Applicant applicant);
    void AddLoanApplication(LoanApplication application);
    void AddLoan(Loan loan);
    void AddRepayment(Repayment repayment);
    void AddCreditAssessment(CreditAssessment assessment);
    void AddAuditLog(AuditLog auditLog);
    void RemoveLoanApplication(LoanApplication application);
    void RemoveLoan(Loan loan);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
