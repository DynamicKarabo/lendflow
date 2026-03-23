using System;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Models;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;

namespace LendFlow.Application.Common.Interfaces;

public interface IAppDbContext
{
    Task<Applicant?> GetApplicantAsync(Guid id, CancellationToken ct);
    Task<LoanApplication?> GetLoanApplicationAsync(Guid id, CancellationToken ct);
    Task<PagedResult<LoanApplication>> GetLoanApplicationsAsync(LoanApplicationStatus? status, int pageNumber, int pageSize, CancellationToken ct);
    void AddApplicant(Applicant applicant);
    void AddLoanApplication(LoanApplication application);
    Task<int> SaveChangesAsync(CancellationToken ct);
}
