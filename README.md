# Cloud Observability Demo

A .NET 8 synthetic API that demonstrates production-minded observability: health/readiness probes, distributed traces, structured logs, custom metrics, KQL investigation queries, and a SEV2 runbook.

## Run locally

```powershell
dotnet run --project src/CloudObservabilityDemo
```

```powershell
curl http://localhost:5000/health
curl http://localhost:5000/ready
curl "http://localhost:5000/api/work?delayMs=250"
curl http://localhost:5000/api/fail
```

Container build:

```powershell
docker build -t cloud-observability-demo .
docker run --rm -p 8080:8080 cloud-observability-demo
```

## Telemetry configuration

The service emits OpenTelemetry traces and metrics for ASP.NET Core, outbound HTTP calls, runtime signals, `demo.work.requests`, `demo.work.duration`, and `demo.failures.controlled`. Console logs are JSON and include structured properties.

Set either or both variables in the host environment; neither is required for local execution.

```powershell
$env:APPLICATIONINSIGHTS_CONNECTION_STRING = "InstrumentationKey=...;IngestionEndpoint=https://..."
$env:OTEL_EXPORTER_OTLP_ENDPOINT = "http://localhost:4317"
```

`APPLICATIONINSIGHTS_CONNECTION_STRING` enables Azure Monitor/Application Insights trace and metric export. `OTEL_EXPORTER_OTLP_ENDPOINT` sends traces, metrics, and logs to an OTLP collector. Do not commit connection strings.

## Incident demonstration

1. Start the service with Application Insights or an OTLP backend configured.
2. Create normal load: `1..20 | % { curl "http://localhost:5000/api/work?delayMs=100" }`.
3. Create a controlled error: `1..5 | % { curl http://localhost:5000/api/fail }`.
4. Use `kql/requests.kql` and `kql/failures.kql` to identify the error-rate increase and correlated operations.
5. Follow `docs/INCIDENT_RUNBOOK.md`; use `docs/APP_INSIGHTS_WORKBOOK.md` to create the supporting workbook.

## Lab limits

This is a synthetic API, not a production platform. It has no authentication, persistence, distributed deployment, alert routing, SLO enforcement, or live Azure resources. The controlled failure endpoint is deliberately exposed only for a safe demo environment; protect or remove it in any real deployment.
