using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using MediatR;

namespace LendFlow.Application.Queries.GetLoans;

public record LoanListItemDto(
    Guid Id,
    Guid ApplicationId,
    decimal PrincipalAmount,
    decimal InterestRate,
    int TermMonths,
    string Status,
    decimal OutstandingBalance,
    DateTimeOffset? DisbursementDate,
    DateOnly MaturityDate,
    DateTimeOffset CreatedAt);

public record GetLoansResult(
    List<LoanListItemDto> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);

public class GetLoansQueryHandler : IRequestHandler<GetLoansQuery, GetLoansResult>
{
    private readonly IAppDbContext _context;

    public GetLoansQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<GetLoansResult> Handle(GetLoansQuery request, CancellationToken ct)
    {
        var result = await _context.GetLoansAsync(request.Status, request.PageNumber, request.PageSize, ct);

        return new GetLoansResult(
            Items: result.Items.Select(MapToDto).ToList(),
            TotalCount: result.TotalCount,
            PageNumber: result.PageNumber,
            PageSize: result.PageSize
        );
    }

    private static LoanListItemDto MapToDto(Loan loan) => new(
        Id: loan.Id,
        ApplicationId: loan.ApplicationId,
        PrincipalAmount: loan.PrincipalAmount,
        InterestRate: loan.InterestRate,
        TermMonths: loan.TermMonths,
        Status: loan.Status.ToString(),
        OutstandingBalance: loan.OutstandingBalance,
        DisbursementDate: loan.DisbursementDate,
        MaturityDate: loan.MaturityDate,
        CreatedAt: loan.CreatedAt
    );
}
