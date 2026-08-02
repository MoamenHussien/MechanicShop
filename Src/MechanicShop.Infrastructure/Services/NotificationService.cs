using System.Net;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

public sealed class NotificationService(ILogger<NotificationService> logger, IOptions<MailSettings> mailsettings) : INotificationService
{
    public async Task SendEmailAsync(string CustomerEmail, string CustomerName, string subject, string body, CancellationToken cancellationToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Mechanic Shop", "no-reply@mechanicshop.com"));
        message.To.Add(new MailboxAddress(CustomerName, CustomerEmail));
        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body,
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(mailsettings.Value.Host, mailsettings.Value.Port, MailKit.Security.SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(mailsettings.Value.Username, mailsettings.Value.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }

    public async Task SendSmsAsync(string CustomerPhoneNum, string CustomerName, string body, CancellationToken cancellationToken)
    {
        var masked = CustomerPhoneNum.Length >= 4
            ? new string('*', CustomerPhoneNum.Length - 4) + CustomerPhoneNum[^4..]
            : "****";

        logger.LogInformation("[SMS] To: {cusotmername} With Phone Num : {Phone} | Message: {Message}", CustomerName, masked, body);

        // Simulated SMS send
        await Task.CompletedTask;
    }
}
