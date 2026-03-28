using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using LendFlow.Application.Common.Interfaces;
using LendFlow.Domain.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LendFlow.Infrastructure.Services;

public class ServiceBusPublisher : IEventPublisher
{
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusPublisher> _logger;
    private readonly bool _isEnabled;

    public ServiceBusPublisher(IConfiguration configuration, ILogger<ServiceBusPublisher> logger)
    {
        _logger = logger;
        
        var connectionString = configuration.GetConnectionString("ServiceBus");
        var queueName = configuration["ServiceBus:QueueName"] ?? "lendflow-events";
        
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("Service Bus connection string not configured - events will not be published");
            _isEnabled = false;
            return;
        }

        _isEnabled = true;
        var client = new ServiceBusClient(connectionString);
        _sender = client.CreateSender(queueName);
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IDomainEvent
    {
        if (!_isEnabled)
        {
            _logger.LogDebug("Service Bus disabled - skipping publish for {EventType}", typeof(T).Name);
            return;
        }

        try
        {
            var message = new ServiceBusMessage
            {
                Body = new BinaryData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event))),
                ContentType = "application/json",
                Subject = typeof(T).Name,
                MessageId = Guid.NewGuid().ToString()
            };

            await _sender.SendMessageAsync(message, ct);
            _logger.LogDebug("Published event {EventType} to Service Bus", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", typeof(T).Name);
            throw;
        }
    }
}
