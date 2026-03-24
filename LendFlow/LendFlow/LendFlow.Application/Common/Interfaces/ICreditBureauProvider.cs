namespace LendFlow.Application.Common.Interfaces;

public interface ICreditBureauProvider
{
    Task<CreditBureauReport> GetReportAsync(string idNumber, Guid tenantId, CancellationToken ct = default);
}

public record CreditBureauReport(
    bool HasReport,
    int? Score,
    string? AccountStatus,
    List<CreditBureauAccount> Accounts,
    List<string> AdverseRemarks
);

public record CreditBureauAccount(
    string Provider,
    decimal Balance,
    decimal MonthlyPayment,
    int DelinquencyDays,
    DateTime OpenedDate
);
