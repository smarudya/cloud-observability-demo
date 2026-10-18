using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CloudObservabilityDemo;

public static class Observability
{
    public const string ServiceName = "cloud-observability-demo";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName, "1.0.0");
    public static readonly Counter<long> WorkRequests = Meter.CreateCounter<long>("demo.work.requests");
    public static readonly Histogram<double> WorkDuration = Meter.CreateHistogram<double>("demo.work.duration", "ms");
    public static readonly Counter<long> ControlledFailures = Meter.CreateCounter<long>("demo.failures.controlled");
}
