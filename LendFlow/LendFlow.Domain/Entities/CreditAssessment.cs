using System;
using LendFlow.Domain.Common;

namespace LendFlow.Domain.Entities;

public class CreditAssessment : BaseAuditableEntity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ApplicationId { get; private set; }
    public int Score { get; private set; }
    public string RiskBand { get; private set; } = string.Empty;
    public string? FactorBreakdown { get; private set; }
    public DateTimeOffset AssessedAt { get; private set; }

    private CreditAssessment() { }

    public static CreditAssessment Create(
        Guid tenantId,
        Guid applicationId,
        int score,
        string riskBand,
        string? factorBreakdown = null)
    {
        return new CreditAssessment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ApplicationId = applicationId,
            Score = score,
            RiskBand = riskBand,
            FactorBreakdown = factorBreakdown,
            AssessedAt = DateTimeOffset.UtcNow
        };
    }

    public static string DetermineRiskBand(int score)
    {
        return score switch
        {
            > 650 => "Low",
            >= 550 and <= 650 => "Medium",
            _ => "High"
        };
    }
}
