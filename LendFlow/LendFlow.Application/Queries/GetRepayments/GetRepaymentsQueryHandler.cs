using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using MediatR;

namespace LendFlow.Application.Queries.GetRepayments;

public record RepaymentDto(
    Guid Id,
    int InstallmentNumber,
    decimal AmountDue,
    decimal? AmountPaid,
    DateOnly DueDate,
    DateTimeOffset? PaidDate,
    string Status,
    string? PaymentReference,
    DateTimeOffset CreatedAt);

public class GetRepaymentsQueryHandler : IRequestHandler<GetRepaymentsQuery, List<RepaymentDto>>
{
    private readonly IAppDbContext _context;

    public GetRepaymentsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RepaymentDto>> Handle(GetRepaymentsQuery request, CancellationToken ct)
    {
        var repayments = await _context.GetRepaymentsByLoanIdAsync(request.LoanId, ct);

        return repayments.Select(r => new RepaymentDto(
            Id: r.Id,
            InstallmentNumber: r.InstallmentNumber,
            AmountDue: r.AmountDue,
            AmountPaid: r.AmountPaid,
            DueDate: r.DueDate,
            PaidDate: r.PaidDate,
            Status: r.Status.ToString(),
            PaymentReference: r.PaymentReference,
            CreatedAt: r.CreatedAt
        )).ToList();
    }
}
