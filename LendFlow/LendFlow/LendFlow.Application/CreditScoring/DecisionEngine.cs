using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;

namespace LendFlow.Application.CreditScoring;

public class DecisionEngine : IDecisionEngine
{
    private const decimal MinMonthlyIncome = 2500m;
    private const decimal MaxDtiRatio = 0.4m;

    public Task<DecisionResult> EvaluateAsync(
        Applicant applicant, 
        LoanApplication application, 
        int creditScore, 
        string riskBand, 
        CancellationToken ct = default)
    {
        var rejectionReasons = new List<string>();

        if (applicant.MonthlyIncome < MinMonthlyIncome)
        {
            rejectionReasons.Add($"Monthly income below minimum threshold (R{MinMonthlyIncome:N0})");
        }

        if (!IsAffordable(applicant, application))
        {
            rejectionReasons.Add("Proposed repayment exceeds 40% of monthly income (NCA affordability)");
        }

        if (applicant.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18)))
        {
            rejectionReasons.Add("Applicant is under 18 years old");
        }

        if (rejectionReasons.Any())
        {
            return Task.FromResult(new DecisionResult(
                IsApproved: false,
                Decision: "Rejected",
                Reason: string.Join("; ", rejectionReasons),
                Score: creditScore,
                RiskBand: riskBand,
                RejectionReasons: rejectionReasons
            ));
        }

        return creditScore switch
        {
            > 650 => Task.FromResult(new DecisionResult(
                IsApproved: true,
                Decision: "Approved",
                Reason: "Score exceeds approval threshold",
                Score: creditScore,
                RiskBand: riskBand,
                RejectionReasons: new List<string>()
            )),
            >= 550 => Task.FromResult(new DecisionResult(
                IsApproved: false,
                Decision: "UnderReview",
                Reason: "Score requires manual underwriter review",
                Score: creditScore,
                RiskBand: riskBand,
                RejectionReasons: new List<string>()
            )),
            _ => Task.FromResult(new DecisionResult(
                IsApproved: false,
                Decision: "Rejected",
                Reason: $"Credit score {creditScore} below minimum threshold (550)",
                Score: creditScore,
                RiskBand: riskBand,
                RejectionReasons: new List<string> { $"Credit score {creditScore} too low" }
            ))
        };
    }

    private static bool IsAffordable(Applicant applicant, LoanApplication application)
    {
        var monthlyIncome = applicant.MonthlyIncome;
        if (monthlyIncome == 0) return false;

        var proposedRepayment = CalculateMonthlyPayment(
            application.RequestedAmount, 
            0.28m, 
            application.RequestedTermMonths);

        var dti = proposedRepayment / monthlyIncome;
        return dti <= MaxDtiRatio;
    }

    private static decimal CalculateMonthlyPayment(decimal principal, decimal annualRate, int months)
    {
        var monthlyRate = annualRate / 12;
        if (monthlyRate == 0) return principal / months;
        
        var rate = (double)monthlyRate;
        var monthlyPayment = (decimal)(rate * Math.Pow(1 + rate, months)) / 
                           ((decimal)Math.Pow(1 + rate, months) - 1);
        return principal * monthlyPayment;
    }
}
