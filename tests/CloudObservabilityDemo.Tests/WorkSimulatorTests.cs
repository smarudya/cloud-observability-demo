using CloudObservabilityDemo;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CloudObservabilityDemo.Tests;

public sealed class WorkSimulatorTests
{
    private readonly WorkSimulator _simulator = new(NullLogger<WorkSimulator>.Instance);

    [Fact]
    public async Task ExecuteAsync_ReturnsRequestedDelayAndOperationId()
    {
        var result = await _simulator.ExecuteAsync(0, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.OperationId);
        Assert.Equal(0, result.RequestedDelayMs);
        Assert.True(result.DurationMs >= 0);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10_001)]
    public async Task ExecuteAsync_RejectsInvalidDelay(int delayMs)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _simulator.ExecuteAsync(delayMs, CancellationToken.None));
    }
}
