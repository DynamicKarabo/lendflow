using LendFlow.Domain.Enums;
using MediatR;

namespace LendFlow.Application.Queries.GetLoans;

public record GetLoansQuery(
    LoanStatus? Status,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<GetLoansResult>;
