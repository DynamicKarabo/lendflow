using LendFlow.Domain.Events;

namespace LendFlow.Application.Common.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IDomainEvent;
}
