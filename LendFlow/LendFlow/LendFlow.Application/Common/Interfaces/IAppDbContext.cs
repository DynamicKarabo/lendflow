using System;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Domain.Entities;

namespace LendFlow.Application.Common.Interfaces;

public interface IAppDbContext
{
    Task<Applicant?> GetApplicantAsync(Guid id, CancellationToken ct);
    void AddLoanApplication(LoanApplication application);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
