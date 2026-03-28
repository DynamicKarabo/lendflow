using System;

namespace LendFlow.Domain.Events;

public record LoanApplicationSubmittedEvent(Guid ApplicationId, Guid TenantId) : IDomainEvent;
