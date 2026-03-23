using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using MediatR;

namespace LendFlow.Application.Commands.MakeDecision;

public class MakeDecisionCommandHandler : IRequestHandler<MakeDecisionCommand, MakeDecisionResult>
{
    private readonly IAppDbContext _context;
    private const decimal DefaultInterestRate = 0.28m;

    public MakeDecisionCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<MakeDecisionResult> Handle(MakeDecisionCommand request, CancellationToken ct)
    {
        var application = await _context.GetLoanApplicationAsync(request.ApplicationId, ct);
        if (application == null)
            throw new InvalidOperationException($"Application {request.ApplicationId} not found.");

        if (application.Status != Domain.Enums.LoanApplicationStatus.UnderReview)
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
        }
        else
        {
            application.Reject(request.Reason);
        }

        await _context.SaveChangesAsync(ct);

        return new MakeDecisionResult(
            ApplicationId: application.Id,
            Decision: decision == "approved" ? "Approved" : "Rejected",
            LoanId: loan?.Id
        );
    }
}
