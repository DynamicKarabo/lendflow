using FluentValidation;

namespace LendFlow.Application.Commands.CreateApplicant;

public class CreateApplicantCommandValidator : AbstractValidator<CreateApplicantCommand>
{
    public CreateApplicantCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.IdNumber).NotEmpty().Length(13);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.EmploymentStatus).NotEmpty().MaximumLength(50);
        RuleFor(x => x.MonthlyIncome).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MonthlyExpenses).GreaterThanOrEqualTo(0);
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
