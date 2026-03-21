using System;

namespace LendFlow.Domain.Events;

public record LoanApplicationCreatedEvent(Guid ApplicationId) : IDomainEvent;
