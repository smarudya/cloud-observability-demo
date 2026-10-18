using Azure.Monitor.OpenTelemetry.Exporter;
using CloudObservabilityDemo;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;

    if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var endpoint))
    {
        options.AddOtlpExporter(exporter => exporter.Endpoint = endpoint);
    }
});

builder.Services.AddHealthChecks()
    .AddCheck<ReadinessHealthCheck>("readiness", tags: ["ready"]);
builder.Services.AddSingleton<WorkSimulator>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(Observability.ServiceName, serviceVersion: serviceVersion)
        .AddAttributes([
            new KeyValuePair<string, object>("deployment.environment",
                builder.Environment.EnvironmentName)
        ]))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSource(Observability.ActivitySource.Name);

        if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var endpoint))
        {
            tracing.AddOtlpExporter(exporter => exporter.Endpoint = endpoint);
        }

        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            tracing.AddAzureMonitorTraceExporter(exporter =>
                exporter.ConnectionString = appInsightsConnectionString);
        }
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter(Observability.Meter.Name);

        if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var endpoint))
        {
            metrics.AddOtlpExporter(exporter => exporter.Endpoint = endpoint);
        }

        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            metrics.AddAzureMonitorMetricExporter(exporter =>
                exporter.ConnectionString = appInsightsConnectionString);
        }
    });

var app = builder.Build();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = WriteHealthResponse
});
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = WriteHealthResponse
});

app.MapGet("/api/work", async (
    int? delayMs,
    WorkSimulator simulator,
    ILoggerFactory loggerFactory,
    CancellationToken cancellationToken) =>
{
    var requestedDelay = delayMs ?? Random.Shared.Next(50, 251);
    var logger = loggerFactory.CreateLogger("WorkEndpoint");

    try
    {
        var result = await simulator.ExecuteAsync(requestedDelay, cancellationToken);
        return Results.Ok(result);
    }
    catch (ArgumentOutOfRangeException exception)
    {
        logger.LogWarning(exception, "Rejected synthetic work request with delay {DelayMs}", requestedDelay);
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["delayMs"] = [exception.Message]
        });
    }
});

app.MapGet("/api/fail", (int? statusCode, ILoggerFactory loggerFactory) =>
{
    var requestedStatus = statusCode ?? StatusCodes.Status500InternalServerError;
    if (requestedStatus is < 400 or > 599)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["statusCode"] = ["statusCode must be between 400 and 599."]
        });
    }

    Observability.ControlledFailures.Add(1, new KeyValuePair<string, object?>("http.response.status_code", requestedStatus));
    loggerFactory.CreateLogger("FailureEndpoint").LogError(
        "Controlled failure generated for observability demonstration with status {StatusCode}",
        requestedStatus);
    return Results.Problem(
        title: "Controlled demo failure",
        detail: "This endpoint intentionally returns an error for dashboard and alert demonstrations.",
        statusCode: requestedStatus);
});

app.Run();

static Task WriteHealthResponse(HttpContext context, HealthReport report) =>
    context.Response.WriteAsJsonAsync(new
    {
        status = report.Status.ToString(),
        timestampUtc = DateTimeOffset.UtcNow,
        checks = report.Entries.ToDictionary(
            entry => entry.Key,
            entry => new { status = entry.Value.Status.ToString(), entry.Value.Description })
    });

public partial class Program;
