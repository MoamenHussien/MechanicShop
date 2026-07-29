public interface INotificationService
{
    Task SendEmailAsync(string CustomerEmail, string CustomerName, string subject, string body, CancellationToken cancellationToken);
    Task SendSmsAsync(string CustomerPhoneNum, string CustomerName, string body, CancellationToken cancellationToken);
}
