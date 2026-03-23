using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.CreditScoring;
using Microsoft.Extensions.DependencyInjection;

namespace LendFlow.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreditAssessmentService>();
        services.AddScoped<IDecisionEngine, DecisionEngine>();
        
        services.AddScoped<ICreditScoringFactor, EmploymentStatusFactor>();
        services.AddScoped<ICreditScoringFactor, IncomeStabilityFactor>();
        services.AddScoped<ICreditScoringFactor, DebtToIncomeFactor>();
        services.AddScoped<ICreditScoringFactor, LoanAmountFactor>();
        
        return services;
    }
}
