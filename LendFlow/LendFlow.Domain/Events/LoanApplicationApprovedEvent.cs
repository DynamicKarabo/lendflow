using System;

namespace LendFlow.Domain.Events;

public record LoanApplicationApprovedEvent(Guid ApplicationId, Guid TenantId) : IDomainEvent;
