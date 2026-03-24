using LendFlow.Application.Commands.AssessCredit;
using LendFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LendFlow.Application.Jobs;

public class CreditAssessmentJob
{
    private readonly IMediator _mediator;
    private readonly ILogger<CreditAssessmentJob> _logger;

    public CreditAssessmentJob(IMediator mediator, ILogger<CreditAssessmentJob> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid applicationId, Guid tenantId, CancellationToken ct)
    {
        _logger.LogInformation("Running credit assessment for application {ApplicationId}", applicationId);

        try
        {
            var command = new AssessCreditCommand(tenantId, applicationId);
            var result = await _mediator.Send(command, ct);
            
            _logger.LogInformation("Credit assessment completed for application {ApplicationId}, Score: {Score}, RiskBand: {RiskBand}", 
                applicationId, result.Score, result.RiskBand);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Credit assessment failed for application {ApplicationId}", applicationId);
            throw;
        }
    }
}
