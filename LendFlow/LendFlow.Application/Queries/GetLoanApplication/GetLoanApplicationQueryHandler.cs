using System.Threading;
using System.Threading.Tasks;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Exceptions;
using MediatR;

namespace LendFlow.Application.Queries.GetLoanApplication;

public class GetLoanApplicationQueryHandler : IRequestHandler<GetLoanApplicationQuery, LoanApplicationDto>
{
    private readonly IAppDbContext _dbContext;

    public GetLoanApplicationQueryHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LoanApplicationDto> Handle(GetLoanApplicationQuery query, CancellationToken ct)
    {
        var application = await _dbContext.GetLoanApplicationAsync(query.ApplicationId, ct);
        if (application == null)
        {
            throw new NotFoundException(nameof(LoanApplication), query.ApplicationId);
        }

        return new LoanApplicationDto(
            application.Id,
            application.TenantId,
            application.ApplicantId,
            application.RequestedAmount,
            application.RequestedTermMonths,
            application.Purpose,
            application.Status,
            application.CreditScore,
            application.RiskBand,
            application.DecisionReason,
            application.IdempotencyKey,
            application.CreatedAt,
            application.UpdatedAt
        );
    }
}
