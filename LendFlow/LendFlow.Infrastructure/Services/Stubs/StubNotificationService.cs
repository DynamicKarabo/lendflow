using LendFlow.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace LendFlow.Infrastructure.Services.Stubs;

public class StubNotificationService : INotificationService
{
    private readonly ILogger<StubNotificationService> _logger;

    public StubNotificationService(ILogger<StubNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("SMS stub to {PhoneNumber}: {Message}", 
            phoneNumber.Length > 4 ? "***" + phoneNumber[^4..] : "***", 
            message);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string email, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("Email stub to {Email}: {Subject}", email, subject);
        return Task.CompletedTask;
    }
}
