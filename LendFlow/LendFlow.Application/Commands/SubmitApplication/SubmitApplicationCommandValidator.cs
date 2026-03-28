using FluentValidation;

namespace LendFlow.Application.Commands.SubmitApplication;

public class SubmitApplicationCommandValidator : AbstractValidator<SubmitApplicationCommand>
{
    public SubmitApplicationCommandValidator()
    {
        RuleFor(x => x.RequestedAmount).InclusiveBetween(100m, 50000m);
        RuleFor(x => x.RequestedTermMonths).InclusiveBetween(1, 60);
        RuleFor(x => x.Purpose).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
