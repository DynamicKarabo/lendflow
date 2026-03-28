using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Events;
using Microsoft.Extensions.Logging;

namespace LendFlow.Application.Events;

public class LoanApplicationRejectedHandler : IDomainEventHandler<LoanApplicationRejectedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IAppDbContext _dbContext;
    private readonly ILogger<LoanApplicationRejectedHandler> _logger;

    public LoanApplicationRejectedHandler(
        INotificationService notificationService,
        IAppDbContext dbContext,
        ILogger<LoanApplicationRejectedHandler> logger)
    {
        _notificationService = notificationService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(LoanApplicationRejectedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation("Handling LoanApplicationRejectedEvent for application {ApplicationId}", @event.ApplicationId);

        var application = await _dbContext.GetLoanApplicationAsync(@event.ApplicationId, ct);
        if (application == null) return;

        var applicant = await _dbContext.GetApplicantAsync(application.ApplicantId, ct);
        if (applicant == null) return;

        var message = $"Your loan application #{application.Id} has been declined. " +
                     $"Reason: {application.DecisionReason ?? "Please contact support for more information."}";

        await _notificationService.SendEmailAsync(applicant.Email, "Loan Application Declined", message, ct);
        
        _logger.LogInformation("Sent rejection notification for application {ApplicationId}", @event.ApplicationId);
    }
}
