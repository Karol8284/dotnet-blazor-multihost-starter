# Blazor Multihost Starter

A .NET reference starter and `dotnet new` template for a Blazor Web app and
.NET MAUI Blazor Hybrid app sharing one Razor Class Library.

**Status:** learning/reference starter, not production-ready software.

## What is included

- Web and MAUI hosts sharing one UI library.
- A protected `Tasks` API with owner isolation based on validated Entra
  `tid` + `oid` claims.
- Three explicit data-mode examples:
  - server-centric Tasks at `/`;
  - device-local IndexedDB at `/tasks/local`;
  - manual offline synchronization at `/tasks/offline`.
- WebAssembly and Interactive Server Web modes selected independently with
  `Web:Interactivity`.
- Entra/MSAL for WebAssembly and MAUI, plus OIDC/cookie for Interactive Server.
- PostgreSQL, EF Core migrations, paginated endpoints, process-local rate
  limiting, health probes, and versioned offline conflict handling.
- Domain, Application, Infrastructure, API contracts, clients, shared UI, and
  focused test projects.
- An installable template and CI checks for fresh generated projects.
- Optional Bicep and GitHub Actions scaffolding for an Azure showcase.

The identity and database providers are intentionally opinionated: Microsoft
Entra ID and PostgreSQL. Other providers are not configuration switches.

## Quick start

### Review the client-local page

Requirements: .NET SDK 10. Install the matching MAUI workload separately only
when running the native client.

```powershell
dotnet tool restore
dotnet run --project .\src\BlazorMultihostStarter.Web
```

Open `http://localhost:5080/tasks/local`. This Development path needs neither
Entra configuration nor PostgreSQL. Server-backed API requests remain disabled
with `503`.

### Generate a project

```powershell
dotnet new install .

dotnet new blazor-multihost-starter `
  --name ContosoTasks `
  --appTitle "Contoso Tasks" `
  --mauiBundleId com.contoso.tasks `
  --tenantId <tenant-guid> `
  --apiClientId <api-client-guid> `
  --webClientId <web-client-guid> `
  --mauiClientId <maui-client-guid> `
  --serverClientId <server-client-guid>
```

`--name` supports PascalCase, kebab-case, and dotted names. Entra values can be
omitted to explore only `/tasks/local` in Development. The template deliberately
has no `ServerClientSecret` parameter; that credential belongs in User Secrets
or deployment secret storage.

For Entra registration, PostgreSQL setup, all host modes, redirect URIs, MAUI
targets, and authenticated smoke tests, use
[Getting started](./docs/GETTING_STARTED.md).

## Build and test

```powershell
dotnet build .\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj --configuration Release
dotnet test .\tests\BlazorMultihostStarter.Domain.Tests\BlazorMultihostStarter.Domain.Tests.csproj --configuration Release
dotnet test .\tests\BlazorMultihostStarter.Application.Tests\BlazorMultihostStarter.Application.Tests.csproj --configuration Release
dotnet test .\tests\BlazorMultihostStarter.Api.IntegrationTests\BlazorMultihostStarter.Api.IntegrationTests.csproj --configuration Release
dotnet test .\tests\BlazorMultihostStarter.Shared.Tests\BlazorMultihostStarter.Shared.Tests.csproj --configuration Release
```

CI additionally builds Windows and Android, tests the API against PostgreSQL,
checks pending EF model changes, and generates/builds fresh templates. The
integration tests use a test-only identity handler; they do not prove live
Entra redirects, native broker behavior, or physical-device persistence.

## Verification boundary

This repository currently verifies automated build and test scenarios. A build
of a MAUI platform is not a runtime smoke test. Real Entra sign-in and
cross-host synchronization must be recorded separately for each platform.

The in-memory API rate limit is per process and is not a global policy for
multi-instance deployment. IndexedDB is device-local storage, not a secure
vault. Production use also requires deployment-specific HTTPS, redirect URIs,
secret storage, backups, monitoring, and a distributed rate-limit strategy.

Current gates and known limitations are tracked in
[Roadmap](./ROADMAP.md).

## Documentation

- [Getting started](./docs/GETTING_STARTED.md) — setup, Entra, database,
  MAUI, and manual acceptance.
- [Architecture](./docs/ARCHITECTURE.md) — projects, dependencies, and
  runtime boundaries.
- [Adding a feature](./docs/ADDING_FEATURES.md) — vertical-slice workflow and
  test checklist.
- [Operations](./docs/OPERATIONS.md) — migrations, health checks,
  backup/restore, and secret rotation.
- [Azure showcase](./docs/AZURE_SHOWCASE.md) — optional infrastructure and
  deployment scaffold, not a production claim.
- [ADRs](./docs/adr/) — accepted decisions and verification boundaries.

## License

See [LICENSE](./LICENSE).
