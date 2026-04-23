using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using MediatR;

namespace LendFlow.Application.Queries.GetLoans;

public record LoanListItemDto(
    Guid Id,
    Guid ApplicationId,
    string ApplicantName,
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

        var applicantIds = result.Items.Select(l => l.ApplicantId).Distinct().ToList();
        var applicants = await Task.WhenAll(applicantIds.Select(id => _context.GetApplicantAsync(id, ct)));
        var applicantMap = applicants.Where(a => a != null).ToDictionary(a => a!.Id, a => $"{a!.FirstName} {a.LastName}");

        return new GetLoansResult(
            Items: result.Items.Select(l => MapToDto(l, applicantMap.GetValueOrDefault(l.ApplicantId, "Unknown"))).ToList(),
            TotalCount: result.TotalCount,
            PageNumber: result.PageNumber,
            PageSize: result.PageSize
        );
    }

    private static LoanListItemDto MapToDto(Loan loan, string applicantName) => new(
        Id: loan.Id,
        ApplicationId: loan.ApplicationId,
        ApplicantName: applicantName,
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
