using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Entities;
using LendFlow.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LendFlow.Application.Jobs;

public class RepaymentReminderJob
{
    private readonly IAppDbContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly ILogger<RepaymentReminderJob> _logger;

    public RepaymentReminderJob(
        IAppDbContext dbContext, 
        INotificationService notificationService,
        ILogger<RepaymentReminderJob> logger)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Running repayment reminder job");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var reminderDate = today.AddDays(3);

        var upcomingRepayments = await _dbContext.GetUpcomingRepaymentsAsync(reminderDate, ct);

        foreach (var repayment in upcomingRepayments)
        {
            var applicant = repayment.Loan?.Applicant;
            if (applicant == null) continue;
            
            var message = $"Reminder: Your loan repayment of R{repayment.AmountDue:N2} is due on {repayment.DueDate:dd MMMM yyyy}.";
            
            await _notificationService.SendSmsAsync(applicant.PhoneNumber, message, ct);
            _logger.LogInformation("Sent repayment reminder for repayment {RepaymentId} to applicant {ApplicantId}",
                repayment.Id, applicant.Id);
        }

        _logger.LogInformation("Repayment reminder job completed. Sent {Count} reminders", upcomingRepayments.Count);
    }
}
