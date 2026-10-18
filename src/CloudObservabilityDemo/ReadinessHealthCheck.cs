using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CloudObservabilityDemo;

public sealed class ReadinessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(HealthCheckResult.Healthy("Service is ready to accept traffic."));
}
