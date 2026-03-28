using System;

namespace LendFlow.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? PreviousState { get; private set; }
    public string NewState { get; private set; } = string.Empty;
    public string? PerformedBy { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? Metadata { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid tenantId,
        string entityType,
        Guid entityId,
        string action,
        string newState,
        string? performedBy = null,
        string? previousState = null,
        string? metadata = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            PreviousState = previousState,
            NewState = newState,
            PerformedBy = performedBy ?? "system",
            OccurredAt = DateTimeOffset.UtcNow,
            Metadata = metadata
        };
    }

    public static AuditLog StateTransition(
        Guid tenantId,
        string entityType,
        Guid entityId,
        string fromState,
        string toState,
        string performedBy)
    {
        return Create(tenantId, entityType, entityId, "StateTransition", toState, performedBy, fromState);
    }
}
