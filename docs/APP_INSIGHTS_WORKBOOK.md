# Suggested Application Insights Workbook

Create a workbook scoped to the Application Insights resource or Log Analytics workspace.

## Overview

- KPI tiles: request count, 5xx rate, p95 request duration, and availability percentage.
- Time range parameter: default 24 hours; auto refresh every five minutes during an incident.
- Filter parameter: operation name, with `/api/work`, `/api/fail`, `/health`, and `/ready` as default choices.

## Tabs

1. **Service health** — request trend, p50/p95 latency, status-code breakdown, and availability-test map. Use `kql/requests.kql` and `kql/availability.kql`.
2. **Failures** — error trend, recent correlated operations, and top messages. Use `kql/failures.kql`.
3. **Dependencies** — dependency failure rate, p95 latency, target, and type. Use `kql/dependencies.kql`.
4. **Operations** — distributed trace table by `operation_Id`, including trace/log detail and deployment markers.

## Alerts

Start with a 5-minute evaluation window and tune against observed baseline:

- 5xx rate above 2% with at least 20 requests.
- p95 request duration above 1 second for 10 minutes.
- Any failed availability test for `/ready` from two locations.

Link alerts to `docs/INCIDENT_RUNBOOK.md` and include service, environment, portal query, and escalation channel in the alert payload.
