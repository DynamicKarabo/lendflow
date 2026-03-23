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

    public Task<Applicant?> GetApplicantAsync(Guid id, CancellationToken ct)
    {
        return Task.FromResult(Applicants.FirstOrDefault(a => a.Id == id));
    }

    public Task<LoanApplication?> GetLoanApplicationAsync(Guid id, CancellationToken ct)
    {
        return Task.FromResult(LoanApplications.FirstOrDefault(a => a.Id == id));
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

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return Task.FromResult(1);
    }
}
