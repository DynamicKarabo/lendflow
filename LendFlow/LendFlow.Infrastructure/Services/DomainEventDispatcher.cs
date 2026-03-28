using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Events;
using Microsoft.Extensions.Logging;

namespace LendFlow.Infrastructure.Services;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        _logger.LogDebug("Dispatching domain event: {EventType}", domainEvent.GetType().Name);
        
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
        var handler = _serviceProvider.GetService(handlerType);

        if (handler is null)
        {
            _logger.LogDebug("No handler found for event {EventType}", eventType.Name);
            return;
        }

        var method = handlerType.GetMethod("HandleAsync");
        if (method is not null)
        {
            await (Task)method.Invoke(handler, new object[] { domainEvent, ct })!;
        }
    }

    public async Task DispatchManyAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        foreach (var @event in domainEvents)
        {
            await DispatchAsync(@event, ct);
        }
    }
}
