public sealed class HealthCheckSettings
{
    public static readonly string Name = "HealthChecks";

    public int MemoryThresholdInMb { get; init; }

    public int MinimumFreeDiskSpaceInGb { get; init; }
}
