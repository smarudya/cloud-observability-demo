# SEV2 Incident Runbook

Use this workflow when customer-facing errors or material latency affect the demo service. A SEV2 requires an incident commander (IC), a technical lead, and a communications owner.

## 1. Detect and declare

1. Confirm the alert using the requests workbook: elevated 5xx rate, p95 latency, or failed availability checks.
2. Open an incident channel and record UTC start time, alert link, impact, affected endpoint, and current owner.
3. Assign the IC. The IC coordinates; the technical lead investigates; the communications owner posts updates.
4. State the initial customer impact and set the next update interval (default: 30 minutes).

## 2. Triage

1. Check `/health` and `/ready` from the deployment environment.
2. Run `kql/requests.kql` to identify the failing operation, status code, and onset.
3. Run `kql/failures.kql`, then pivot on `operation_Id` to correlate logs, traces, and requests.
4. Check `kql/dependencies.kql` for downstream timeouts or elevated dependency latency.
5. Compare the onset with deployments, configuration changes, traffic shifts, and platform events.

## 3. Mitigate

Choose the lowest-risk reversible mitigation:

- Roll back or halt a recent deployment.
- Route traffic away from an unhealthy instance or region.
- Reduce concurrency, apply a feature flag, or degrade a non-critical dependency.
- Scale the service when capacity or saturation evidence supports it.

Record every action, operator, time, and observed effect. Validate recovery with the requests and availability views; do not close on a single successful request.

## 4. Communicate and resolve

1. Post customer-impact updates on the agreed cadence.
2. Declare mitigation when error rate and latency return to normal and availability checks are stable.
3. Monitor for at least 30 minutes, then resolve the incident with an explicit UTC end time.

## 5. Postmortem

Within five business days, publish a blameless postmortem containing timeline, impact, detection gap, root cause, contributing factors, what worked, and corrective actions. Each action needs an owner and due date; track it to closure.
