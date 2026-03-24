using LendFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace LendFlow.Infrastructure.Services.Stubs;

public class StubCreditBureauProvider : ICreditBureauProvider
{
    private readonly ILogger<StubCreditBureauProvider> _logger;

    public StubCreditBureauProvider(ILogger<StubCreditBureauProvider> logger)
    {
        _logger = logger;
    }

    public Task<CreditBureauReport> GetReportAsync(string idNumber, Guid tenantId, CancellationToken ct = default)
    {
        _logger.LogInformation("Credit bureau stub called for {IdNumber} - returning empty report", idNumber[..Math.Min(6, idNumber.Length)] + "***");
        
        return Task.FromResult(new CreditBureauReport(
            HasReport: true,
            Score: 650,
            AccountStatus: "Active",
            Accounts: new List<CreditBureauAccount>(),
            AdverseRemarks: new List<string>()
        ));
    }
}
