# Azure showcase

## Purpose and status

This repository contains optional Bicep and a manually triggered GitHub
Actions workflow for the Web/API host and PostgreSQL database. It is a
deployment scaffold for a controlled learning demonstration, not evidence of a
successful Azure deployment and not a production reference architecture.

No subscription, region, SKU, cost, live sign-in, backup restore, or deployment
result is assumed by this repository. Those items must be verified for the
target Azure subscription immediately before use.

## Included resources

- App Service plan and Linux Web App;
- Azure Database for PostgreSQL Flexible Server;
- Log Analytics workspace and Application Insights;
- explicit Bicep output for the application URL and database host;
- a workflow with OIDC login, migration generation, migration application,
  publish, deployment, and health checks.

The template uses a single Web/API process. It has no high-availability,
multi-region, auto-scaling, or disaster-recovery design.

## Cost and subscription gate

Do not create resources from an estimated dollar amount in documentation.
Before every run:

1. Check current Azure prices and regional availability for the selected SKUs.
2. Confirm subscription quotas and applicable student/free-account policies.
3. Set a hard spending alert or budget where the subscription supports it.
4. Record the expected hourly and monthly cost plus storage/egress assumptions.
5. Obtain explicit approval for the resources and cost.
6. Plan teardown immediately after the demonstration.

Stopping PostgreSQL may pause compute cost, but it does not guarantee removal
of all storage, backup, monitoring, or App Service charges. Deleting the
resource group is the authoritative cleanup path for resources owned by this
showcase.

## Prerequisites

### Azure

- A subscription approved for the intended resources.
- A service principal/workload identity that can deploy only the required
  resources.
- Federated credentials for GitHub Actions OIDC.
- PostgreSQL and App Service availability in the selected region.
- A reviewed Entra configuration and deployment redirect URI.

### GitHub Actions configuration

The workflow currently expects:

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`
- `AZURE_POSTGRES_PASSWORD`
- `ENTRA_TENANT_ID`
- `ENTRA_API_CLIENT_ID`
- `ENTRA_WEB_CLIENT_ID`
- `ENTRA_MAUI_CLIENT_ID`

Validate each value's owner, scope, and rotation policy. OIDC credentials are
short-lived, but the database password remains a stored secret and must be
managed accordingly.

## Deployment scope

The workflow is intentionally manual (`workflow_dispatch`). It:

1. logs in to Azure through OIDC;
2. creates or reuses the named resource group;
3. deploys Bicep;
4. generates an idempotent EF migration script;
5. applies the script with `psql`;
6. publishes and deploys the Web host;
7. checks `/healthz` and `/health/ready`.

The workflow currently supplies WebAssembly defaults. Interactive Server also
needs a confidential client ID and secret; do not select that mode until the
deployment configuration explicitly provides and verifies both.

A passing health probe proves only that the process can respond and that the
configured database can be reached. It does not prove Entra sign-in, user
authorization, backup/restore, redirect behavior, MAUI access, or operational
readiness.

## Required post-deployment checks

Record `PASS`, `FAIL`, `BLOCKED`, or `NOT RUN` for:

- HTTPS liveness and readiness;
- EF migration history and pending model changes;
- real Entra Web sign-in and protected Tasks flow;
- same-user API access from a MAUI client if that scenario is claimed;
- log behavior without tokens or secrets;
- database backup and restore into a separate target;
- rollback decision and procedure;
- current cost and budget alert;
- teardown.

Do not retain personal or production-like data in the showcase.

## Teardown

After the approved test, delete the dedicated resource group:

```bash
az group delete \
  --name <approved-resource-group> \
  --yes \
  --no-wait
```

Confirm deletion completion and review remaining resources, role assignments,
secrets, logs, and backups. Resource-group deletion is a destructive operation
and requires its own confirmation at execution time.

## Production boundary

The files in this showcase are not a substitute for application-specific
threat modeling, private networking, HA/DR, RPO/RTO, observability, incident
response, data residency, or compliance requirements. Move to a production
design only after those requirements are explicit.
