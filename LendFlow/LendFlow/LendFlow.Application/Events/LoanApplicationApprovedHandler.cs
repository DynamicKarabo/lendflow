using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Events;
using Microsoft.Extensions.Logging;

namespace LendFlow.Application.Events;

public class LoanApplicationApprovedHandler : IDomainEventHandler<LoanApplicationApprovedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IAppDbContext _dbContext;
    private readonly ILogger<LoanApplicationApprovedHandler> _logger;

    public LoanApplicationApprovedHandler(
        INotificationService notificationService,
        IAppDbContext dbContext,
        ILogger<LoanApplicationApprovedHandler> logger)
    {
        _notificationService = notificationService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task HandleAsync(LoanApplicationApprovedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation("Handling LoanApplicationApprovedEvent for application {ApplicationId}", @event.ApplicationId);

        var application = await _dbContext.GetLoanApplicationAsync(@event.ApplicationId, ct);
        if (application == null) return;

        var applicant = await _dbContext.GetApplicantAsync(application.ApplicantId, ct);
        if (applicant == null) return;

        var message = $"Your loan application #{application.Id} has been approved! " +
                     $"You will receive R{application.RequestedAmount:N2} shortly.";

        await _notificationService.SendEmailAsync(applicant.Email, "Loan Approved", message, ct);
        await _notificationService.SendSmsAsync(applicant.PhoneNumber, message, ct);

        _logger.LogInformation("Sent approval notification for application {ApplicationId}", @event.ApplicationId);
    }
}
