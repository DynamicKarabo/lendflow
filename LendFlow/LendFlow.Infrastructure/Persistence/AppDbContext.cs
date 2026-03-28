using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.Common.Models;
using LendFlow.Domain.Common;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LendFlow.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    private readonly ICurrentTenantService _tenantService;
    private readonly ICurrentUserService _userService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService tenantService,
        ICurrentUserService userService) : base(options)
    {
        _tenantService = tenantService;
        _userService = userService;
    }

    public DbSet<Applicant> Applicants => Set<Applicant>();
    public DbSet<LoanApplication> LoanApplications => Set<LoanApplication>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Repayment> Repayments => Set<Repayment>();
    public DbSet<CreditAssessment> CreditAssessments => Set<CreditAssessment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    IQueryable<LoanApplication> IAppDbContext.LoanApplications => LoanApplications.AsNoTracking();
    IQueryable<Loan> IAppDbContext.Loans => Loans.AsNoTracking();
    IQueryable<Repayment> IAppDbContext.Repayments => Repayments.AsNoTracking();
    IQueryable<Applicant> IAppDbContext.Applicants => Applicants.AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Multi-tenant global query filters
        modelBuilder.Entity<Applicant>()
            .HasQueryFilter(a => a.TenantId == _tenantService.TenantId);

        modelBuilder.Entity<LoanApplication>()
            .HasQueryFilter(l => l.TenantId == _tenantService.TenantId);

        modelBuilder.Entity<Loan>()
            .HasQueryFilter(l => l.TenantId == _tenantService.TenantId);

        modelBuilder.Entity<Repayment>()
            .HasQueryFilter(r => r.TenantId == _tenantService.TenantId);

        modelBuilder.Entity<CreditAssessment>()
            .HasQueryFilter(c => c.TenantId == _tenantService.TenantId);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                entry.Entity.CreatedBy = _userService.UserId ?? "system";
            }
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                entry.Entity.UpdatedBy = _userService.UserId ?? "system";
            }
        }
        return await base.SaveChangesAsync(ct);
    }

    public Task<Applicant?> GetApplicantAsync(Guid id, CancellationToken ct)
    {
        return Applicants.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public Task<LoanApplication?> GetLoanApplicationAsync(Guid id, CancellationToken ct)
    {
        return LoanApplications.FirstOrDefaultAsync(a => a.Id == id, ct);
    }

    public async Task<PagedResult<LoanApplication>> GetLoanApplicationsAsync(LoanApplicationStatus? status, int pageNumber, int pageSize, CancellationToken ct)
    {
        var query = LoanApplications.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<LoanApplication>(items, totalCount, pageNumber, pageSize);
    }

    public void AddApplicant(Applicant applicant)
    {
        Applicants.Add(applicant);
    }

    public void AddLoanApplication(LoanApplication application)
    {
        LoanApplications.Add(application);
    }

    public Task<Loan?> GetLoanAsync(Guid id, CancellationToken ct)
    {
        return Loans.FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<PagedResult<Loan>> GetLoansAsync(LoanStatus? status, int pageNumber, int pageSize, CancellationToken ct)
    {
        var query = Loans.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Loan>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<List<Repayment>> GetRepaymentsByLoanIdAsync(Guid loanId, CancellationToken ct)
    {
        return await Repayments
            .AsNoTracking()
            .Where(r => r.LoanId == loanId)
            .OrderBy(r => r.InstallmentNumber)
            .ToListAsync(ct);
    }

    public void AddLoan(Loan loan)
    {
        Loans.Add(loan);
        foreach (var repayment in loan.Repayments)
        {
            Repayments.Add(repayment);
        }
    }

    public void AddRepayment(Repayment repayment)
    {
        Repayments.Add(repayment);
    }

    public void AddCreditAssessment(CreditAssessment assessment)
    {
        CreditAssessments.Add(assessment);
    }

    public void AddAuditLog(AuditLog auditLog)
    {
        AuditLogs.Add(auditLog);
    }

    public void RemoveLoanApplication(LoanApplication application)
    {
        LoanApplications.Remove(application);
    }

    public void RemoveLoan(Loan loan)
    {
        Loans.Remove(loan);
    }

    public async Task<List<Repayment>> GetUpcomingRepaymentsAsync(DateOnly reminderDate, CancellationToken ct)
    {
        return await Repayments
            .Include(r => r.Loan)
                .ThenInclude(l => l!.Applicant)
            .Where(r => r.Status == RepaymentStatus.Scheduled && r.DueDate == reminderDate)
            .ToListAsync(ct);
    }

    public async Task<List<LoanApplication>> GetOldRejectedApplicationsAsync(DateTime cutoffDate, CancellationToken ct)
    {
        return await LoanApplications
            .Where(la => la.Status == LoanApplicationStatus.Rejected && la.UpdatedAt < cutoffDate)
            .ToListAsync(ct);
    }

    public async Task<List<Loan>> GetOldSettledLoansAsync(DateTime cutoffDate, CancellationToken ct)
    {
        return await Loans
            .Where(l => l.Status == LoanStatus.Settled && l.UpdatedAt < cutoffDate)
            .ToListAsync(ct);
    }

    public async Task<List<Repayment>> GetLateRepaymentsAsync(DateOnly today, CancellationToken ct)
    {
        return await Repayments
            .Where(r => r.Status == RepaymentStatus.Scheduled && r.DueDate < today)
            .ToListAsync(ct);
    }
}
