using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using MediatR;

namespace LendFlow.Application.Queries.GetLoan;

public record LoanDto(
    Guid Id,
    Guid TenantId,
    Guid ApplicationId,
    Guid ApplicantId,
    string ApplicantName,
    decimal PrincipalAmount,
    decimal InterestRate,
    int TermMonths,
    string RepaymentFrequency,
    DateTimeOffset? DisbursementDate,
    DateOnly MaturityDate,
    decimal OutstandingBalance,
    decimal MonthlyInstallment,
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
            throw new LendFlow.Domain.Exceptions.NotFoundException(nameof(Loan), request.Id);

        var applicant = await _context.GetApplicantAsync(loan.ApplicantId, ct);
        var applicantName = applicant != null ? $"{applicant.FirstName} {applicant.LastName}" : "Unknown";

        return MapToDto(loan, applicantName);
    }

    private static LoanDto MapToDto(Loan loan, string applicantName) => new(
        Id: loan.Id,
        TenantId: loan.TenantId,
        ApplicationId: loan.ApplicationId,
        ApplicantId: loan.ApplicantId,
        ApplicantName: applicantName,
        PrincipalAmount: loan.PrincipalAmount,
        InterestRate: loan.InterestRate,
        TermMonths: loan.TermMonths,
        RepaymentFrequency: loan.RepaymentFrequency,
        DisbursementDate: loan.DisbursementDate,
        MaturityDate: loan.MaturityDate,
        OutstandingBalance: loan.OutstandingBalance,
        MonthlyInstallment: loan.GetMonthlyInstallment(),
        Status: loan.Status.ToString(),
        CreatedAt: loan.CreatedAt
    );
}
