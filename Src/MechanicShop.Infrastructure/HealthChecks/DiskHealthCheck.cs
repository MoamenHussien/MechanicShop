using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace MechanicShop.Infrastructure.HealthChecks;

public sealed class DiskHealthCheck(IOptions<HealthCheckSettings> options) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var drive = DriveInfo.GetDrives()
            .FirstOrDefault(d =>
                d.IsReady &&
                d.Name == Path.GetPathRoot(AppContext.BaseDirectory));

        if (drive is null)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("Application drive was not found."));
        }

        var minimumSpace = options.Value.MinimumFreeDiskSpaceInGb * 1024L * 1024L * 1024L;

        if (drive.AvailableFreeSpace < minimumSpace)
        {
            return Task.FromResult(
                HealthCheckResult.Degraded(
                    $"Available disk space is {drive.AvailableFreeSpace / 1024 / 1024} MB."));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy(
                $"Available disk space is {drive.AvailableFreeSpace / 1024 / 1024} MB."));
    }
}