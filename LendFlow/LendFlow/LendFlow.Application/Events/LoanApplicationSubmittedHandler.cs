using LendFlow.Application.Common.Interfaces;
using LendFlow.Application.Jobs;
using LendFlow.Domain.Events;
using Microsoft.Extensions.Logging;

namespace LendFlow.Application.Events;

public class LoanApplicationSubmittedHandler : IDomainEventHandler<LoanApplicationSubmittedEvent>
{
    private readonly CreditAssessmentJob _creditAssessmentJob;
    private readonly ILogger<LoanApplicationSubmittedHandler> _logger;

    public LoanApplicationSubmittedHandler(CreditAssessmentJob creditAssessmentJob, ILogger<LoanApplicationSubmittedHandler> logger)
    {
        _creditAssessmentJob = creditAssessmentJob;
        _logger = logger;
    }

    public async Task HandleAsync(LoanApplicationSubmittedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation("Handling LoanApplicationSubmittedEvent for application {ApplicationId}", @event.ApplicationId);
        
        await _creditAssessmentJob.ExecuteAsync(@event.ApplicationId, @event.TenantId, ct);
    }
}
