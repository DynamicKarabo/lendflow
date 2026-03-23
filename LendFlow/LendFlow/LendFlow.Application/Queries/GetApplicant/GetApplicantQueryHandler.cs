using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Exceptions;
using MediatR;

namespace LendFlow.Application.Queries.GetApplicant;

public class GetApplicantQueryHandler : IRequestHandler<GetApplicantQuery, ApplicantDto>
{
    private readonly IAppDbContext _dbContext;

    public GetApplicantQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApplicantDto> Handle(GetApplicantQuery query, CancellationToken ct)
    {
        var applicant = await _dbContext.GetApplicantAsync(query.ApplicantId, ct);
        if (applicant == null)
        {
            throw new NotFoundException(nameof(Applicant), query.ApplicantId);
        }

        return new ApplicantDto(
            applicant.Id,
            applicant.TenantId,
            applicant.FirstName,
            applicant.LastName,
            applicant.IdNumber,
            applicant.PhoneNumber,
            applicant.Email,
            applicant.DateOfBirth,
            applicant.EmploymentStatus,
            applicant.MonthlyIncome,
            applicant.MonthlyExpenses,
            applicant.CreatedAt,
            applicant.UpdatedAt
        );
    }
}
