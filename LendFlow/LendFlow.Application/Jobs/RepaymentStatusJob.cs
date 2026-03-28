using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LendFlow.Application.Jobs;

public class RepaymentStatusJob
{
    private readonly IAppDbContext _dbContext;
    private readonly ILogger<RepaymentStatusJob> _logger;

    public RepaymentStatusJob(IAppDbContext dbContext, ILogger<RepaymentStatusJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Running repayment status check job");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var lateRepayments = await _dbContext.GetLateRepaymentsAsync(today, ct);

        foreach (var repayment in lateRepayments)
        {
            var daysLate = today.DayNumber - repayment.DueDate.DayNumber;
            var newStatus = daysLate > 30 ? RepaymentStatus.Missed : RepaymentStatus.Late;
            repayment.UpdateStatus(newStatus);
            _logger.LogInformation("Repayment {RepaymentId} marked as {Status}, {DaysLate} days late",
                repayment.Id, newStatus, daysLate);
        }

        await _dbContext.SaveChangesAsync(ct);
        
        _logger.LogInformation("Repayment status check completed. Updated {Count} repayments", lateRepayments.Count);
    }
}
