using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using MediatR;

namespace LendFlow.Application.Commands.MakeDecision;

public class MakeDecisionCommandHandler : IRequestHandler<MakeDecisionCommand, MakeDecisionResult>
{
    private readonly IAppDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private const decimal DefaultInterestRate = 0.28m;

    public MakeDecisionCommandHandler(IAppDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<MakeDecisionResult> Handle(MakeDecisionCommand request, CancellationToken ct)
    {
        var application = await _context.GetLoanApplicationAsync(request.ApplicationId, ct);
        if (application == null)
            throw new InvalidOperationException($"Application {request.ApplicationId} not found.");

        if (application.Status != LoanApplicationStatus.UnderReview)
            throw new InvalidOperationException("Application must be under review to make a decision.");

        var decision = request.Decision.ToLower();
        if (decision != "approved" && decision != "rejected")
            throw new InvalidOperationException("Decision must be 'approved' or 'rejected'.");

        Loan? loan = null;

        if (decision == "approved")
        {
            application.Approve(request.Reason);
            
            var maturityDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(application.RequestedTermMonths));
            
            loan = Loan.Create(
                request.TenantId,
                application.Id,
                application.ApplicantId,
                application.RequestedAmount,
                DefaultInterestRate,
                application.RequestedTermMonths,
                maturityDate);
            
            _context.AddLoan(loan);
            
            GenerateRepaymentSchedule(loan);
        }
        else
        {
            application.Reject(request.Reason);
        }

        await _context.SaveChangesAsync(ct);

        await _eventDispatcher.DispatchManyAsync(application.DomainEvents, ct);
        application.ClearDomainEvents();

        return new MakeDecisionResult(
            ApplicationId: application.Id,
            Decision: decision == "approved" ? "Approved" : "Rejected",
            LoanId: loan?.Id
        );
    }

    private void GenerateRepaymentSchedule(Loan loan)
    {
        var monthlyInstallment = loan.GetMonthlyInstallment();
        var dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));

        for (int i = 1; i <= loan.TermMonths; i++)
        {
            var repayment = Repayment.Create(
                loan.TenantId,
                loan.Id,
                i,
                monthlyInstallment,
                dueDate);
            
            loan.AddRepayment(repayment);
            dueDate = dueDate.AddMonths(1);
        }
    }
}
