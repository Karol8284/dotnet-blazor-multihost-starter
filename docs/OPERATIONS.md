# Operations and runbooks

These runbooks describe the starter's explicit operational boundaries. Initial
host setup, Entra registration, and platform redirect URIs are in
[Getting started](./GETTING_STARTED.md).

## 1. Database migrations

### Invariant

The Web host does not apply migrations during startup. Production migrations
must be reviewed and executed explicitly before or during a controlled
deployment.

Verify that the EF model matches the checked-in migrations:

```bash
dotnet tool restore
dotnet ef migrations has-pending-model-changes \
  --project src/BlazorMultihostStarter.Infrastructure/BlazorMultihostStarter.Infrastructure.csproj \
  --startup-project src/BlazorMultihostStarter.Web/BlazorMultihostStarter.Web.csproj \
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext
```

For local Development:

```bash
docker compose up -d --wait postgres
dotnet ef database update \
  --project src/BlazorMultihostStarter.Infrastructure \
  --startup-project src/BlazorMultihostStarter.Web \
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext
```

For a controlled deployment, generate and review an idempotent script:

```bash
dotnet ef migrations script --idempotent \
  --project src/BlazorMultihostStarter.Infrastructure \
  --startup-project src/BlazorMultihostStarter.Web \
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext \
  --output artifacts/migration.sql
```

Do not apply the script until its target database, transaction/locking impact,
backup, rollback decision, and traffic behavior have been reviewed.

## 2. Health and readiness

| Endpoint | Scope | Expected response |
| --- | --- | --- |
| `/healthz` | Process self-check tagged `live` | `200 Healthy` while the process can serve requests |
| `/health/live` | Same live self-check | `200 Healthy` |
| `/health/ready` | PostgreSQL connectivity when the database is configured | `200 Healthy` or `503 Unhealthy` |

In Development, readiness reports healthy when database infrastructure is
unconfigured so the client-local route can be explored. Outside Development,
startup already requires valid API and database configuration.

For a failed readiness check:

1. Verify process logs without exposing tokens or connection strings.
2. Verify host, port, TLS, credentials, firewall rules, and DNS.
3. Check whether the database is reachable and has the expected schema.
4. Confirm that the API is configured for the intended environment.
5. Restore readiness before treating a deployment as successful.

## 3. Local PostgreSQL backup and restore

Use a disposable environment for restore exercises. The following commands use
the repository's default local Compose service and credentials.

Create a logical backup:

```bash
docker compose exec -T postgres \
  pg_dump -U starter -d blazor_multihost_starter -F c -f /tmp/starter-backup.dump
docker compose cp postgres:/tmp/starter-backup.dump ./starter-backup.dump
```

Restore into a separate disposable database:

```bash
docker compose exec -T postgres \
  createdb -U starter starter_restore_test
docker compose cp ./starter-backup.dump postgres:/tmp/starter-backup.dump
docker compose exec -T postgres \
  pg_restore -U starter -d starter_restore_test --clean --if-exists \
  /tmp/starter-backup.dump
```

Verify the restored schema and application path explicitly. Passing readiness
alone does not prove that domain data or constraints were restored correctly.
Remove the disposable database after verification:

```bash
docker compose exec -T postgres \
  dropdb -U starter starter_restore_test
```

For a managed production database, use the provider's backup policy and restore
procedure. Do not assume this local Compose exercise is a production recovery
test.

## 4. Secret boundaries and rotation

| Value | Classification | Storage |
| --- | --- | --- |
| Tenant/client IDs, scope names, redirect URIs | Public protocol metadata | Client or host configuration as required |
| `Authentication:Entra:ServerClientSecret` | Confidential | User Secrets or deployment secret manager |
| `ConnectionStrings:DefaultConnection` | Confidential for real environments | Host configuration or deployment secret manager |

To rotate the Interactive Server confidential client secret:

1. Create and safely record a replacement in the Entra app registration.
2. Update the secret manager or deployment setting.
3. Restart/redeploy the affected host.
4. Verify Interactive Server sign-in and protected reads/writes.
5. Revoke the previous secret only after the replacement is verified.

Never commit secrets, template them into generated clients, or include them in
logs, screenshots, issue comments, or chat.

## 5. API throttling and diagnostics

The Tasks API uses a fixed window of 60 requests per minute, partitioned by
authenticated `tid:oid` or the unauthenticated client address. Rejection is
`429 Too Many Requests`.

The limiter is in-memory and per process. A multi-instance deployment needs a
gateway or distributed policy; it must not be described as globally enforcing
the same quota.

Diagnostic checks should record status code, correlation evidence, and
environment without logging access tokens, claims that identify a person,
connection strings, or confidential client secrets.

## 6. Release evidence

Record each host and scenario separately as `PASS`, `FAIL`, `BLOCKED`, or
`NOT RUN`. Retain the command or manual procedure and the verified commit.
Do not convert a CI build, synthetic identity test, or health probe into a
claim of real authentication or end-to-end platform behavior.
