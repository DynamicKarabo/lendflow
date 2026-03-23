using FluentValidation;

namespace LendFlow.Application.Queries.GetLoanApplications;

public class GetLoanApplicationsQueryValidator : AbstractValidator<GetLoanApplicationsQuery>
{
    public GetLoanApplicationsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
