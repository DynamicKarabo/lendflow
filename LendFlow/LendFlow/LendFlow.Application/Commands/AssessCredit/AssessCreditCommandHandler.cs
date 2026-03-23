using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.CreditScoring;
using LendFlow.Domain.Entities;
using MediatR;

namespace LendFlow.Application.Commands.AssessCredit;

public class AssessCreditCommandHandler : IRequestHandler<AssessCreditCommand, AssessCreditResult>
{
    private readonly IAppDbContext _context;
    private readonly CreditAssessmentService _assessmentService;
    private readonly IDecisionEngine _decisionEngine;
    private const decimal DefaultInterestRate = 0.28m;

    public AssessCreditCommandHandler(
        IAppDbContext context, 
        CreditAssessmentService assessmentService,
        IDecisionEngine decisionEngine)
    {
        _context = context;
        _assessmentService = assessmentService;
        _decisionEngine = decisionEngine;
    }

    public async Task<AssessCreditResult> Handle(AssessCreditCommand request, CancellationToken ct)
    {
        var application = await _context.GetLoanApplicationAsync(request.ApplicationId, ct);
        if (application == null)
            throw new InvalidOperationException($"Application {request.ApplicationId} not found.");

        var applicant = await _context.GetApplicantAsync(application.ApplicantId, ct);
        if (applicant == null)
            throw new InvalidOperationException($"Applicant {application.ApplicantId} not found.");

        var (score, riskBand, breakdown) = await _assessmentService.AssessAsync(applicant, application, ct);
        
        var decisionResult = await _decisionEngine.EvaluateAsync(applicant, application, score, riskBand, ct);
        
        var assessment = CreditAssessment.Create(
            request.TenantId,
            application.Id,
            score,
            riskBand,
            CreditAssessmentService.SerializeBreakdown(breakdown));
        
        _context.AddCreditAssessment(assessment);
        
        application.SetAssessmentResult(score, riskBand);
        
        if (decisionResult.Decision == "Approved")
        {
            application.Approve(decisionResult.Reason);
            
            var maturityDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(application.RequestedTermMonths));
            
            var loan = Loan.Create(
                request.TenantId,
                application.Id,
                application.ApplicantId,
                application.RequestedAmount,
                DefaultInterestRate,
                application.RequestedTermMonths,
                maturityDate);
            
            _context.AddLoan(loan);
        }
        else if (decisionResult.Decision == "Rejected")
        {
            application.Reject(decisionResult.Reason);
        }
        else
        {
            application.Review();
        }
        
        await _context.SaveChangesAsync(ct);

        return new AssessCreditResult(
            AssessmentId: assessment.Id,
            Score: score,
            RiskBand: riskBand,
            Decision: decisionResult.Decision,
            Reason: decisionResult.Reason,
            FactorBreakdown: breakdown.Select(b => $"{b.FactorName}: {b.Score}/{b.MaxScore} - {b.Reason}").ToList()
        );
    }
}
