using MediatR;

namespace LendFlow.Application.Queries.GetRepayments;

public record GetRepaymentsQuery(Guid LoanId) : IRequest<List<RepaymentDto>>;
