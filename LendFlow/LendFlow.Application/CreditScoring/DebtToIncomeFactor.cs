using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;

namespace LendFlow.Application.CreditScoring;

public class DebtToIncomeFactor : ICreditScoringFactor
{
    public string Name => "DebtToIncome";
    public int Weight => 30;

    public Task<CreditScoreFactorResult> EvaluateAsync(Applicant applicant, LoanApplication application, CancellationToken ct = default)
    {
        var monthlyIncome = applicant.MonthlyIncome;
        if (monthlyIncome == 0)
        {
            return Task.FromResult(new CreditScoreFactorResult(
                FactorName: Name,
                Score: 0,
                MaxScore: 100,
                Reason: "No income recorded"
            ));
        }

        var monthlyPayment = CalculateMonthlyPayment(application.RequestedAmount, 0.28m, application.RequestedTermMonths);
        var dti = (monthlyPayment / monthlyIncome) * 100;

        var score = dti switch
        {
            <= 10 => 100,
            <= 20 => 85,
            <= 30 => 70,
            <= 40 => 50,
            _ => 0
        };

        return Task.FromResult(new CreditScoreFactorResult(
            FactorName: Name,
            Score: score,
            MaxScore: 100,
            Reason: $"DTI ratio: {dti:F1}% (proposed repayment: R{monthlyPayment:N0})"
        ));
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
