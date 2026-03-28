using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;

namespace LendFlow.Application.CreditScoring;

public class LoanAmountFactor : ICreditScoringFactor
{
    public string Name => "LoanAmount";
    public int Weight => 20;

    public Task<CreditScoreFactorResult> EvaluateAsync(Applicant applicant, LoanApplication application, CancellationToken ct = default)
    {
        var amount = application.RequestedAmount;
        var income = applicant.MonthlyIncome;
        
        var loanToIncomeRatio = income > 0 ? (amount / income) : 100;
        
        var score = loanToIncomeRatio switch
        {
            <= 0.5m => 100,
            <= 1.0m => 85,
            <= 2.0m => 70,
            <= 3.0m => 50,
            _ => 25
        };

        return Task.FromResult(new CreditScoreFactorResult(
            FactorName: Name,
            Score: score,
            MaxScore: 100,
            Reason: $"Loan to income ratio: {loanToIncomeRatio:F1}x monthly income"
        ));
    }
}
