# Blazor Multihost Starter

A small .NET starter for a Blazor Web client and a .NET MAUI Blazor Hybrid app
that share one Razor UI library and call the same API.

**Status:** learning/reference starter, not production-ready software.

## What is included

- One solution with Web, MAUI, shared UI, API, persistence, and test projects.
- One authenticated, per-user `Tasks` module organized as vertical slices:
  - list tasks;
  - create a task;
  - complete a task.
- Owner-scoped list pagination (25 items by default, at most 100) and a
  per-user API rate limit (60 requests per minute, in-memory per API process).
- A single Razor Class Library (RCL) used by both WebAssembly and MAUI.
- PostgreSQL through EF Core. The Web host serves the Web client and the API;
  MAUI calls that same API.
- Microsoft Entra ID/MSAL sign-in for Web and MAUI clients; the API validates
  delegated access tokens and scopes task access to the signed-in tenant/user.
- Domain, application, infrastructure, API contracts, and client code have
  explicit dependency boundaries.
- Unit, API integration, and shared-component tests.

The Web interactivity decision is recorded in
[ADR-0001](./docs/adr/0001-web-interactivity.md). The implementation uses
Interactive WebAssembly because the browser and MAUI app are both API clients.
The shared screen appears after the WebAssembly client starts; the server does
not prerender the Tasks page.
Authentication and client/server execution boundaries are described in
[ADR-0002](./docs/adr/0002-authentication-and-execution-modes.md). Tasks are
server-centric: neither client is trusted to enforce access or data ownership.
Local-first write queues and offline synchronization are not implemented.

This is a learning/reference starter, not a production-ready identity solution.
Production use requires your own Entra app registrations, verified redirect
URIs, HTTPS deployment, and platform-specific sign-in smoke tests.

## Prerequisites

- .NET SDK 10.0.402 or a later feature band in the .NET 10 line.
- Docker Desktop or another Docker-compatible engine to run the local
  PostgreSQL database.
- For the MAUI app, the workload and platform tooling for the target you want
  to run. On Windows, the Windows target is the simplest starting point.

Install workloads for the current machine if needed:

```powershell
dotnet workload restore .\BlazorMultihostStarter.sln
dotnet tool restore
```

## Run the Web app and API

### Configure Microsoft Entra ID first

Register three Entra applications: an API that exposes the delegated
`Tasks.ReadWrite` scope, a single-page Web client, and a native MAUI public
client. Grant both clients the API's delegated scope. Use the API application's
client ID as the API audience. The clients are public applications: do not
create or ship a client secret.

Set the server-side API values with User Secrets:

```powershell
$webProject = '.\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj'
dotnet user-secrets set 'Authentication:Entra:TenantId' '<tenant-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:ApiClientId' '<api-client-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:WebClientId' '<web-client-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:MauiClientId' '<maui-client-guid>' --project $webProject
```

Set the **public** Web client values in
`src/BlazorMultihostStarter.Web.Client/wwwroot/appsettings.Development.json`:

```json
{
  "Entra": {
    "TenantId": "<tenant-guid>",
    "ClientId": "<web-client-guid>",
    "ApiClientId": "<api-client-guid>",
    "ApiScopeName": "Tasks.ReadWrite"
  }
}
```

Set the same tenant/API values and the MAUI client ID in
`src/BlazorMultihostStarter.Maui/Resources/Raw/appsettings.json`. Also replace
`MauiEntraRegistration.ClientId` in
`src/BlazorMultihostStarter.Maui/Authentication/MauiEntraOptions.cs` with the
MAUI app's client ID. Register the MAUI Android redirect
`msal<MAUI_CLIENT_ID>://auth`, the iOS redirect
`msauth.com.example.blazormultihoststarter://auth` (also used by Mac Catalyst),
and the Windows broker redirect
`ms-appx-web://microsoft.aad.brokerplugin/<MAUI_CLIENT_ID>` for that public
client. Match the Android intent filter and iOS/Mac Catalyst URL schemes in
the platform files to the registered client ID and bundle ID. Register the Web redirect
`http://localhost:5080/authentication/login-callback` for local development
and HTTPS redirects for deployment.

Tenant and client IDs are public identifiers, not secrets. The app never
accepts anonymous Tasks API requests if Entra configuration is missing or
invalid; API startup fails closed. The integration tests use a test-only
authentication handler and do not contact a real tenant.
Outside Development, the Web host also rejects wildcard `AllowedHosts` and the
sample local database password.

### Start the application

From the repository root, with the Entra client settings above configured:

```powershell
docker compose up -d --wait postgres
dotnet ef database update `
  --project .\src\BlazorMultihostStarter.Infrastructure\BlazorMultihostStarter.Infrastructure.csproj `
  --startup-project .\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj `
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext
dotnet run --project .\src\BlazorMultihostStarter.Web
```

Open `http://localhost:5080`. The development HTTP profile listens on all
interfaces so an Android emulator can reach the API. The project PostgreSQL
container is exposed on host port `5433` so it can coexist with a local
PostgreSQL server on `5432`; use the browser-facing
`localhost` URL on the development machine. The WebAssembly client and `/api/tasks`
are served by the same Web host, so local Web development doesn't require
separate CORS configuration. In Development, the OpenAPI document is available
at `/openapi/v1.json`.

## Run the MAUI app

Keep PostgreSQL and the Web/API host running. In another terminal, run the MAUI
project for a target supported by your machine, for example on Windows:

```powershell
dotnet run --project .\src\BlazorMultihostStarter.Maui `
  --framework net10.0-windows10.0.19041.0
```

The shared Tasks screen uses the same API client in Web and MAUI. A Debug
build defaults to `http://localhost:5080` on Windows and
`http://10.0.2.2:5080` on the Android emulator. For a physical device, set
`Maui:ApiBaseAddress` to an address reachable from it; `localhost` on a device
is the device itself. Release builds require an HTTPS API address. See
[local setup](./docs/GETTING_STARTED.md) for platform-specific guidance.

## Build and test

```powershell
dotnet build .\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj `
  --configuration Release
dotnet build .\src\BlazorMultihostStarter.Maui\BlazorMultihostStarter.Maui.csproj `
  --framework net10.0-windows10.0.19041.0 `
  --configuration Release
dotnet test .\tests\BlazorMultihostStarter.Domain.Tests\BlazorMultihostStarter.Domain.Tests.csproj `
  --configuration Release
dotnet test .\tests\BlazorMultihostStarter.Application.Tests\BlazorMultihostStarter.Application.Tests.csproj `
  --configuration Release
dotnet test .\tests\BlazorMultihostStarter.Api.IntegrationTests\BlazorMultihostStarter.Api.IntegrationTests.csproj `
  --configuration Release
dotnet test .\tests\BlazorMultihostStarter.Shared.Tests\BlazorMultihostStarter.Shared.Tests.csproj `
  --configuration Release
```

API integration tests use SQLite in-memory by default, so they run without
Docker. Set `TEST_POSTGRES_CONNECTION` to a disposable PostgreSQL connection
string to run the same API flow against PostgreSQL and apply the real migration.
Never point integration tests at a database containing data you need to keep.
Tests use a test-only authentication handler; they verify API authorization
policies and owner isolation but do not contact a live Entra tenant or prove
the real MSAL redirect/broker flow.

## Architecture and adding features

- [Architecture](./docs/ARCHITECTURE.md) — projects, responsibilities, and dependency direction.
- [Adding a feature](./docs/ADDING_FEATURES.md) — the Tasks VSA golden path and test checklist.
- [Getting started](./docs/GETTING_STARTED.md) — local database, API URL, and MAUI targets.
- [Roadmap](./ROADMAP.md) — V1 scope and deliberate follow-up work.

This starter borrows architectural practices from
[dotnet-react-starter](https://github.com/Karol-Malecki-dev/dotnet-react-starter);
it intentionally does not copy that application's password/refresh-token
session model, business features, deployment stack, or operational tooling.
It uses Entra/MSAL public-client flows because WebAssembly and MAUI have
different token-acquisition and platform-storage requirements.