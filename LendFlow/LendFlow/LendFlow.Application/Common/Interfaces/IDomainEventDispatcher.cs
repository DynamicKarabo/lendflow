using LendFlow.Domain.Events;

namespace LendFlow.Application.Common.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);
    Task DispatchManyAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
}
