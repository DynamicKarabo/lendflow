namespace LendFlow.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default);
    Task SendEmailAsync(string email, string subject, string body, CancellationToken ct = default);
}

public record NotificationMessage(
    NotificationType Type,
    string Recipient,
    string Subject,
    string Body,
    Guid? CorrelationId = null
);

public enum NotificationType
{
    Sms,
    Email
}
