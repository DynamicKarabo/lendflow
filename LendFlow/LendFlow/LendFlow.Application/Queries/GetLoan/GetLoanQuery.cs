using MediatR;

namespace LendFlow.Application.Queries.GetLoan;

public record GetLoanQuery(Guid Id) : IRequest<LoanDto>;
