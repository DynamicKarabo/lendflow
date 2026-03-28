using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LendFlow.Application.Jobs;

public class RetentionCleanupJob
{
    private readonly IAppDbContext _dbContext;
    private readonly ILogger<RetentionCleanupJob> _logger;

    public RetentionCleanupJob(IAppDbContext dbContext, ILogger<RetentionCleanupJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("Running retention cleanup job");

        var cutoffDate = DateTime.UtcNow.AddYears(-5);

        var rejectedApplications = await _dbContext.GetOldRejectedApplicationsAsync(cutoffDate, ct);
        var settledLoans = await _dbContext.GetOldSettledLoansAsync(cutoffDate, ct);

        var rejectedCount = rejectedApplications.Count;
        var settledCount = settledLoans.Count;

        _logger.LogInformation("Retention cleanup: Found {RejectedCount} rejected applications and {SettledCount} settled loans older than 5 years",
            rejectedCount, settledCount);

        foreach (var app in rejectedApplications)
        {
            _dbContext.RemoveLoanApplication(app);
        }
        
        foreach (var loan in settledLoans)
        {
            _dbContext.RemoveLoan(loan);
        }
        
        await _dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Retention cleanup completed. Archived {TotalCount} records", rejectedCount + settledCount);
    }
}
