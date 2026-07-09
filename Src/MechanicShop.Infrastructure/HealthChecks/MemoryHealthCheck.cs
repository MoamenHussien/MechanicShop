using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.HealthChecks;

public sealed class MemoryHealthCheck(IOptions<HealthCheckSettings> options) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var memoryUsed = GC.GetTotalMemory(false);

        var threshold = options.Value.MemoryThresholdInMb * 1024L * 1024L;

        if (memoryUsed > threshold)
        {
            return Task.FromResult(
                HealthCheckResult.Degraded(
                    $"Memory usage is {memoryUsed / 1024 / 1024} MB."));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy(
                $"Memory usage is {memoryUsed / 1024 / 1024} MB."));
    }
}