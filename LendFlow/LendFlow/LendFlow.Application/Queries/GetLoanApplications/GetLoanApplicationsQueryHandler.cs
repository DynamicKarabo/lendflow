using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.Common.Models;
using MediatR;

namespace LendFlow.Application.Queries.GetLoanApplications;

public class GetLoanApplicationsQueryHandler : IRequestHandler<GetLoanApplicationsQuery, PagedResult<LoanApplicationListItemDto>>
{
    private readonly IAppDbContext _dbContext;

    public GetLoanApplicationsQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<LoanApplicationListItemDto>> Handle(GetLoanApplicationsQuery query, CancellationToken ct)
    {
        var paged = await _dbContext.GetLoanApplicationsAsync(query.Status, query.PageNumber, query.PageSize, ct);

        var items = paged.Items
            .Select(a => new LoanApplicationListItemDto(
                a.Id,
                a.ApplicantId,
                a.RequestedAmount,
                a.RequestedTermMonths,
                a.Purpose,
                a.Status,
                a.CreditScore,
                a.RiskBand,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .ToList();

        return new PagedResult<LoanApplicationListItemDto>(items, paged.TotalCount, paged.PageNumber, paged.PageSize);
    }
}
