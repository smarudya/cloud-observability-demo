using CloudObservabilityDemo;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace CloudObservabilityDemo.Tests;

public sealed class ReadinessHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_ReturnsHealthy()
    {
        var check = new ReadinessHealthCheck();
        var context = new HealthCheckContext
        {
            Registration = new HealthCheckRegistration("readiness", check, HealthStatus.Unhealthy, [])
        };

        var result = await check.CheckHealthAsync(context);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }
}
