# Blazor Multihost Starter

A .NET reference starter and `dotnet new` template for Blazor Web and .NET MAUI
Blazor Hybrid apps that share one Razor UI library.

**Status:** learning/reference starter, not production-ready software.

## What is included

- One solution with Web, MAUI, shared UI, API, persistence, and test projects.
- One authenticated, per-user `Tasks` module organized as vertical slices:
  - list tasks;
  - create a task;
  - complete a task.
  - synchronize local snapshots with version-based conflict detection.
- Three separate data-mode examples:
  - server-centric API and PostgreSQL at `/`;
  - client-local IndexedDB at `/tasks/local`;
  - offline-first local queue with explicit synchronization at `/tasks/offline`.
- Web interactivity selectable between Interactive WebAssembly and Interactive
  Server with `Web:Interactivity`.
- Owner-scoped list pagination (25 items by default, at most 100) and a
  per-user API rate limit (60 requests per minute, in-memory per API process).
- A single Razor Class Library (RCL) used by WebAssembly, Interactive Server,
  and MAUI.
- PostgreSQL through EF Core. WebAssembly and MAUI use the protected API;
  Interactive Server calls the same Application use cases inside the Web host.
- Microsoft Entra ID authentication: MSAL public clients for WebAssembly and
  MAUI, and an OIDC cookie session for Interactive Server.
- Domain, application, infrastructure, API contracts, and client code have
  explicit dependency boundaries.
- An installable template that replaces project names and accepts app title,
  MAUI bundle ID, and public Entra application IDs.
- Unit, API integration, and shared-component tests, including versioned
  offline synchronization conflicts.

Web interactivity and data authority are independent choices. The default Web
mode is Interactive WebAssembly; set `Web:Interactivity` to `Server` to use the
cookie-authenticated Interactive Server host. See
[ADR-0001](./docs/adr/0001-web-interactivity.md). The three task data modes and
offline conflict policy are described in
[ADR-0002](./docs/adr/0002-authentication-and-execution-modes.md) and
[ADR-0003](./docs/adr/0003-data-modes-and-offline-conflicts.md).

This is a learning/reference starter, not production-ready software. It is
opinionated around Microsoft Entra ID and PostgreSQL; other identity providers
and databases are not included. Production use requires your own Entra app
registrations, verified redirect URIs, HTTPS deployment, and real
platform-specific sign-in and synchronization smoke tests.

## Use it as a project template

Install the template from the repository root:

```powershell
dotnet new install .
```

Then generate a renamed project:

```powershell
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

Entra IDs may be omitted during generation; unresolved placeholders make the
application fail closed until configured. The template generates fresh MAUI
application and User Secrets GUIDs. `ServerClientSecret` is deliberately not a
template option: set it in User Secrets or deployment secret storage. All
three data-mode examples are included; choose by route and remove examples the
application does not need. The Web rendering mode is selected separately with
`Web:Interactivity` (`WebAssembly` or `Server`).

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

Register an Entra API application that exposes the delegated `Tasks.ReadWrite`
scope, a single-page WebAssembly client, and a native MAUI public client. Grant
both public clients the API's delegated scope. Use the API application's
client ID as the API audience. Public clients must not have a client secret.
If you select Interactive Server, also register a confidential Web client with
the OIDC callback `http://localhost:5080/signin-oidc`. Store its client secret
in User Secrets; never place it in appsettings or source control.

Set the server-side API values with User Secrets:

```powershell
$webProject = '.\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj'
dotnet user-secrets set 'Authentication:Entra:TenantId' '<tenant-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:ApiClientId' '<api-client-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:WebClientId' '<web-client-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:MauiClientId' '<maui-client-guid>' --project $webProject
```

For Interactive Server, also set:

```powershell
dotnet user-secrets set 'Authentication:Entra:ServerClientId' '<server-client-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:ServerClientSecret' '<server-client-secret>' --project $webProject
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

Set the tenant/API/MAUI client IDs in
`src/BlazorMultihostStarter.Maui/Resources/Raw/appsettings.json`. The MAUI
client ID must match `MauiEntraRegistration.ClientId` in
`src/BlazorMultihostStarter.Maui/Authentication/MauiEntraOptions.cs`. Register
the WebAssembly redirect `http://localhost:5080/authentication/login-callback`
and the MAUI Android redirect
`msal<MAUI_CLIENT_ID>://auth`, the iOS redirect
`msauth.com.example.blazormultihoststarter://auth` (also used by Mac Catalyst),
and the Windows broker redirect
`ms-appx-web://microsoft.aad.brokerplugin/<MAUI_CLIENT_ID>` for that public
client. Match the Android intent filter and iOS/Mac Catalyst URL schemes in
the platform files to the registered client ID and bundle ID. For Interactive
Server, register `http://localhost:5080/signin-oidc`. Use HTTPS redirects for
deployment.

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
PostgreSQL server on `5432`. The default WebAssembly client and `/api/tasks`
are served from one origin. To use Interactive Server, set
`Web:Interactivity` to `Server` in Web settings and configure the confidential
server client values above. In Development, OpenAPI is available at
`/openapi/v1.json`.

The shared navigation exposes the server-centric, client-local, and offline
hybrid Tasks examples. IndexedDB data is scoped to the browser/WebView and is
not a secure vault. Offline synchronization requires sign-in; a stale
`BaseVersion` returns `409 Conflict` and the UI asks whether to keep the server
or local snapshot.

## Run the MAUI app

Keep PostgreSQL and the Web/API host running. In another terminal, run the MAUI
project for a target supported by your machine, for example on Windows:

```powershell
dotnet run --project .\src\BlazorMultihostStarter.Maui `
  --framework net10.0-windows10.0.19041.0
```

The server-centric and offline-hybrid examples use the API from MAUI. A Debug
build defaults to `http://localhost:5080` on Windows and
`http://10.0.2.2:5080` on the Android emulator. The client-local example uses
the WebView's IndexedDB and does not contact the API. For a physical device, set
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
policies, owner isolation, local queue behavior, and version conflicts but do
not contact a live Entra tenant or prove real OIDC/MSAL redirects or native
broker flows.

## Architecture and adding features

- [Architecture](./docs/ARCHITECTURE.md) — projects, responsibilities, and dependency direction.
- [Adding a feature](./docs/ADDING_FEATURES.md) — the Tasks VSA golden path and test checklist.
- [Getting started](./docs/GETTING_STARTED.md) — local database, API URL, and MAUI targets.
- [Roadmap](./ROADMAP.md) — current status, verification boundaries, and follow-up work.
- [ADR-0003](./docs/adr/0003-data-modes-and-offline-conflicts.md) — client-local and offline sync semantics.

This starter borrows architectural practices from
[dotnet-react-starter](https://github.com/Karol-Malecki-dev/dotnet-react-starter);
it intentionally does not copy that application's password/refresh-token
session model, business features, deployment stack, or operational tooling.
It uses Entra/MSAL public-client flows for WebAssembly and MAUI because those
hosts have different token-acquisition and platform-storage requirements.
Interactive Server uses a separate OIDC cookie session and a server-side
adapter; it requires a confidential Entra client and secret.