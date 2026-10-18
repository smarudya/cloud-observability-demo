using System.Diagnostics;

namespace CloudObservabilityDemo;

public sealed class WorkSimulator(ILogger<WorkSimulator> logger)
{
    public async Task<WorkResult> ExecuteAsync(int delayMs, CancellationToken cancellationToken)
    {
        if (delayMs is < 0 or > 10_000)
        {
            throw new ArgumentOutOfRangeException(nameof(delayMs), "delayMs must be between 0 and 10000.");
        }

        using var activity = Observability.ActivitySource.StartActivity("demo.work.execute");
        var stopwatch = Stopwatch.StartNew();
        await Task.Delay(delayMs, cancellationToken);
        stopwatch.Stop();

        var durationMs = stopwatch.Elapsed.TotalMilliseconds;
        Observability.WorkRequests.Add(1, new KeyValuePair<string, object?>("work.delay_ms", delayMs));
        Observability.WorkDuration.Record(durationMs);
        logger.LogInformation(
            "Synthetic work completed in {DurationMs}ms with requested delay {RequestedDelayMs}ms",
            durationMs,
            delayMs);

        return new WorkResult(Guid.NewGuid(), delayMs, durationMs);
    }
}

public sealed record WorkResult(Guid OperationId, int RequestedDelayMs, double DurationMs);
