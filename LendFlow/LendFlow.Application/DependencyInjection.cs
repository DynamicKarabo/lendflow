using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.CreditScoring;
using LendFlow.Application.Events;
using LendFlow.Application.Jobs;
using LendFlow.Domain.Events;
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

        services.AddScoped<CreditAssessmentJob>();
        services.AddScoped<RepaymentStatusJob>();
        services.AddScoped<RepaymentReminderJob>();
        services.AddScoped<RetentionCleanupJob>();

        services.AddScoped<IDomainEventHandler<LoanApplicationSubmittedEvent>, LoanApplicationSubmittedHandler>();
        services.AddScoped<IDomainEventHandler<LoanApplicationApprovedEvent>, LoanApplicationApprovedHandler>();
        services.AddScoped<IDomainEventHandler<LoanApplicationRejectedEvent>, LoanApplicationRejectedHandler>();
        
        return services;
    }
}
