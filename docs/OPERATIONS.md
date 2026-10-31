# Operations and Runbooks

This document outlines the operational procedures for the `dotnet-blazor-multihost-starter` template and projects generated from it. It covers development, migration, backup/restore, health monitoring, and security maintenance.

---

## 1. Local Development Runbook

### Prerequisites
- .NET SDK `10.0.402` or later.
- Docker engine for local PostgreSQL.
- Workload `maui-android` or `maui-windows` (only when running native MAUI hosts).

### Quick Start (Server-Centric Mode)
1. **Start PostgreSQL**:
   ```bash
   docker compose up -d postgres
   ```
2. **Apply EF Core Migrations**:
   ```bash
   dotnet ef database update \
     --project src/BlazorMultihostStarter.Infrastructure \
     --startup-project src/BlazorMultihostStarter.Web \
     --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext
   ```
3. **Run Web Host**:
   ```bash
   dotnet run --project src/BlazorMultihostStarter.Web
   ```
   Open `http://localhost:5080`.

### Local-Only Exploration (Without Database or Entra)
- You can run the Web host immediately without starting Docker or configuring Entra:
  ```bash
  dotnet run --project src/BlazorMultihostStarter.Web
  ```
- Navigate to `http://localhost:5080/tasks/local` to use client-side IndexedDB persistence.
- Any request to `/api/*` will safely fail closed with `503 Service Unavailable`.

---

## 2. Database Migration Deployment Runbook

### Invariant: No Automatic Migrations at Startup
The Web host **never** executes `Database.MigrateAsync()` or mutates schema on application startup. Automatic runtime migration in multi-instance or production environments creates race conditions, locks tables unexpectedly, and prevents controlled rollbacks.

### Migration Procedures

#### A. Local Development
Apply migrations directly using the .NET EF tool:
```bash
dotnet ef database update \
  --project src/BlazorMultihostStarter.Infrastructure \
  --startup-project src/BlazorMultihostStarter.Web \
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext
```

#### B. Generating Idempotent SQL Scripts for Production / CI Pipeline
For production deployments, generate a SQL script that checks `__EFMigrationsHistory` before applying each migration:
```bash
dotnet ef migrations script --idempotent \
  --project src/BlazorMultihostStarter.Infrastructure \
  --startup-project src/BlazorMultihostStarter.Web \
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext \
  --output ./artifacts/migration-bundle.sql
```
Review the SQL script before executing it against the production database, or execute it in a deployment pipeline step prior to swapping application traffic.

#### C. Verifying Model State
Ensure there are no pending schema changes before committing code:
```bash
dotnet ef migrations has-pending-model-changes \
  --project src/BlazorMultihostStarter.Infrastructure \
  --startup-project src/BlazorMultihostStarter.Web \
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext
```

---

## 3. Health & Readiness Monitoring Runbook

The Web host exposes three standard HTTP health check endpoints:

| Endpoint | Tag | Check Performed | Expected Response | Usage |
|---|---|---|---|---|
| `/healthz` | `live` | Process liveness (`self`) | `200 OK` ("Healthy") | Orchestrator/K8s liveness probe. Fails only if process is deadlocked or dying. |
| `/health/live` | `live` | Process liveness (`self`) | `200 OK` ("Healthy") | Alternative liveness endpoint. |
| `/health/ready` | `ready` | Database connectivity via `CanConnectAsync()` | `200 OK` ("Healthy") or `503 Service Unavailable` ("Unhealthy") | Readiness probe. Informs load balancers whether traffic can be routed. |

### Development Behavior
In Development mode, if Entra or the database connection string is not configured, `/health/ready` returns `200 OK` ("Healthy") so that client-local UI developers are not blocked. When the database is configured, `/health/ready` actively probes connectivity.

### Troubleshooting Unhealthy Readiness
If `/health/ready` returns `503 Service Unavailable`:
1. Check database connectivity: `docker ps` or verify PostgreSQL network accessibility.
2. Check logs for connection pool exhaustion, timeout, or invalid credentials.
3. Validate connection string host, port, and credentials in User Secrets or environment variables.

---

## 4. PostgreSQL Backup & Restore Runbook

### Backup (Logical Dump)
To create a compressed binary backup of the PostgreSQL database:
```bash
docker exec -t starter-postgres pg_dump \
  -U starter_dev \
  -d starter_db \
  -F c \
  -f /tmp/backup.dump

# Copy backup out of container
docker cp starter-postgres:/tmp/backup.dump ./backup-$(date +%Y%m%d%H%M%S).dump
```

### Restore Procedure
1. Create a clean disposable database or drop the existing schema:
   ```bash
   docker exec -i starter-postgres psql -U starter_dev -d postgres -c "DROP DATABASE IF EXISTS starter_db;"
   docker exec -i starter-postgres psql -U starter_dev -d postgres -c "CREATE DATABASE starter_db;"
   ```
2. Restore from backup dump:
   ```bash
   docker cp ./backup-latest.dump starter-postgres:/tmp/restore.dump
   docker exec -t starter-postgres pg_restore \
     -U starter_dev \
     -d starter_db \
     --clean --if-exists \
     /tmp/restore.dump
   ```
3. Run readiness check to verify data integrity:
   ```bash
   curl -i http://localhost:5080/health/ready
   ```

---

## 5. Security & Secret Rotation Runbook

### Secret Boundaries
- **Public Identifiers**: Tenant ID, Application (Client) IDs, Scope names, and Redirect URIs. These may reside in client appsettings or parameters.
- **Private Credentials**: `Authentication:Entra:ServerClientSecret` (confidential client for Interactive Server) and database connection strings.
  - **NEVER** pass client secrets as template parameters.
  - **NEVER** commit secrets into Git repository or include in client bundles.
  - In development, use `dotnet user-secrets`.
  - In staging/production, use Azure Key Vault, AWS Secrets Manager, or container environment variables.

### Rotating the Entra Confidential Client Secret
1. In the Microsoft Entra admin center, navigate to **App registrations** > Web / Confidential Client.
2. Under **Certificates & secrets**, add a new client secret. Note the expiration date.
3. Update the deployment environment variable or secret store:
   `Authentication__Entra__ServerClientSecret="<new_secret>"`
4. Verify Interactive Server sign-in at `http://localhost:5080/signin-oidc`.
5. In Entra admin center, delete the old secret after zero traffic uses it.

---

## 6. Rate Limiting and API Diagnostics

- The Tasks API is protected by an in-memory fixed-window rate limiter (`tasks-api` policy):
  - **Limit**: 60 requests per minute.
  - **Partitioning**: Authenticated user (`tid:oid`) or remote IP for unauthenticated requests.
  - **Rejection**: Returns `429 Too Many Requests` with a ProblemDetails response.
- In multi-instance or scaled deployments, configure a distributed rate limiter (e.g., Redis rate limiting) or API gateway (e.g., Azure API Management) to enforce global limits.
