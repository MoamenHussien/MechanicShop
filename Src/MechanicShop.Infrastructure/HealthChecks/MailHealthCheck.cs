using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

public sealed class MailHealthCheck(IOptions<MailSettings> options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();

            // 1. الاتصال والتأكد إن السيرفر متاح والبورت مفتوح ويدعم التشفير
            await client.ConnectAsync(
                options.Value.Host,
                options.Value.Port,
                MailKit.Security.SecureSocketOptions.StartTls,
                cancellationToken);

            // 2. فصل الاتصال بأمان
            await client.DisconnectAsync(true, cancellationToken);

            return HealthCheckResult.Healthy("SMTP server is reachable and accepting connections.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SMTP server is unreachable.", ex);
        }
    }
}
