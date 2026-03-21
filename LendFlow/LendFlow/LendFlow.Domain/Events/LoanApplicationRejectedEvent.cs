using System;

namespace LendFlow.Domain.Events;

public record LoanApplicationRejectedEvent(Guid ApplicationId, string Reason) : IDomainEvent;
