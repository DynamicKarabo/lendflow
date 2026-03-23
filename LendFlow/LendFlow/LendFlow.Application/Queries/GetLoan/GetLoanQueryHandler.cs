using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using MediatR;

namespace LendFlow.Application.Queries.GetLoan;

public record LoanDto(
    Guid Id,
    Guid TenantId,
    Guid ApplicationId,
    Guid ApplicantId,
    decimal PrincipalAmount,
    decimal InterestRate,
    int TermMonths,
    string RepaymentFrequency,
    DateTimeOffset? DisbursementDate,
    DateOnly MaturityDate,
    decimal OutstandingBalance,
    string Status,
    DateTimeOffset CreatedAt);

public class GetLoanQueryHandler : IRequestHandler<GetLoanQuery, LoanDto>
{
    private readonly IAppDbContext _context;

    public GetLoanQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<LoanDto> Handle(GetLoanQuery request, CancellationToken ct)
    {
        var loan = await _context.GetLoanAsync(request.Id, ct);
        if (loan == null)
            throw new InvalidOperationException($"Loan {request.Id} not found.");

        return MapToDto(loan);
    }

    private static LoanDto MapToDto(Loan loan) => new(
        Id: loan.Id,
        TenantId: loan.TenantId,
        ApplicationId: loan.ApplicationId,
        ApplicantId: loan.ApplicantId,
        PrincipalAmount: loan.PrincipalAmount,
        InterestRate: loan.InterestRate,
        TermMonths: loan.TermMonths,
        RepaymentFrequency: loan.RepaymentFrequency,
        DisbursementDate: loan.DisbursementDate,
        MaturityDate: loan.MaturityDate,
        OutstandingBalance: loan.OutstandingBalance,
        Status: loan.Status.ToString(),
        CreatedAt: loan.CreatedAt
    );
}
