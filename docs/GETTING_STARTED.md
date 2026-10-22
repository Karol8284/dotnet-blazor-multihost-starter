# Getting started

## Requirements

- .NET SDK `10.0.402` or a later SDK in the .NET 10 feature band.
- Docker Desktop or a compatible Docker engine for the server-centric and
  synchronized examples; client-local Development does not require it.
- A MAUI workload and platform tooling for the host you want to run.

Check the SDK and workloads:

```powershell
dotnet --version
dotnet workload list
```

## Generate a project

From this repository's root, install the template:

```powershell
dotnet new install .
```

Generate a project with a new name and app identifiers:

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

Entra IDs may be omitted while exploring in Development: client-local pages
remain usable, while sign-in is unavailable and API requests return `503`.
Production continues to fail closed. The template creates new User Secrets and
MAUI application GUIDs. It never accepts a client secret as a template
argument. All data-mode examples are included; choose by route and remove
examples the application does not need.

## Start PostgreSQL

PostgreSQL is required for server-centric Tasks and successful hybrid sync.
Skip this section if you only want to inspect the client-local route.

Copy `.env.example` to `.env`, then start the local database:

```powershell
Copy-Item .\.env.example .\.env
docker compose up -d --wait postgres
```

The development connection string matches the sample Compose credentials and
connects to host port `5433` (the container listens on `5432`). Port `5433`
avoids conflicts with a PostgreSQL server on the default port. These credentials
are disposable local-development values, not production secrets.

## Run client-local in Development

From the repository root, run the Web host and open the local-only route:

```powershell
dotnet run --project .\src\BlazorMultihostStarter.Web
```

Open `http://localhost:5080/tasks/local`. No Entra registration, PostgreSQL
container, or API is required for this route. The Web host logs that its Tasks
API is disabled and returns `503` to server-backed API calls; it does not
enable anonymous API access.

MAUI Debug can also run the `/tasks/local` page without Entra configuration or
a running Web host. For server-centric tasks and offline synchronization,
configure Entra and PostgreSQL below.

## Configure Entra

Register an Entra API application with a delegated `Tasks.ReadWrite` scope, a
WebAssembly SPA client, and a MAUI public client. Grant both public clients the
API scope. Public clients must not have a client secret. Interactive Server
additionally requires a confidential Web client with the OIDC callback
`http://localhost:5080/signin-oidc`.

Set server-side public identifiers using User Secrets:

```powershell
$webProject = '.\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj'
dotnet user-secrets set 'Authentication:Entra:TenantId' '<tenant-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:ApiClientId' '<api-client-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:WebClientId' '<web-client-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:MauiClientId' '<maui-client-guid>' --project $webProject
```

For Interactive Server, also set the confidential client values:

```powershell
dotnet user-secrets set 'Authentication:Entra:ServerClientId' '<server-client-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:ServerClientSecret' '<server-client-secret>' --project $webProject
```

Set WebAssembly public values in
`src/BlazorMultihostStarter.Web.Client/wwwroot/appsettings.Development.json`
and MAUI values in
`src/BlazorMultihostStarter.Maui/Resources/Raw/appsettings.json`. The MAUI
client ID must match `MauiEntraRegistration.ClientId`.

Set `Web:Interactivity` in
`src/BlazorMultihostStarter.Web/appsettings.Development.json` to `WebAssembly`
or `Server`. WebAssembly uses the SPA callback
`http://localhost:5080/authentication/login-callback`. Interactive Server uses
the confidential client's OIDC callback `http://localhost:5080/signin-oidc`.
MAUI redirects are `msal<MAUI_CLIENT_ID>://auth` on Android,
`msauth.<MAUI_BUNDLE_ID>://auth` on iOS/Mac Catalyst, and
`ms-appx-web://microsoft.aad.brokerplugin/<MAUI_CLIENT_ID>` on Windows. Match
native URL schemes to the registered MAUI client and bundle IDs. Use HTTPS
redirects for deployment.

## Apply migrations and run Web/API

Start PostgreSQL, then run from the repository root:

```powershell
dotnet tool restore
dotnet ef database update `
  --project .\src\BlazorMultihostStarter.Infrastructure\BlazorMultihostStarter.Infrastructure.csproj `
  --startup-project .\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj `
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext
dotnet run --project .\src\BlazorMultihostStarter.Web
```

Open `http://localhost:5080`. The development HTTP profile listens on all
interfaces so an Android emulator can reach the API. In WebAssembly mode, the
browser client and API share one origin. Outside Development, startup rejects
wildcard `AllowedHosts` and the sample PostgreSQL password; provide explicit
host names and a managed database connection string.

The shared navigation contains three data-mode examples:

- `/` — server-centric Tasks; the API/database are authoritative.
- `/tasks/local` — client-local Tasks stored in IndexedDB on the current
  browser/WebView, without server synchronization.
- `/tasks/offline` — local-first edits with manual sync to the API. Stale
  versions return `409 Conflict`; choose whether to keep the server or local
  snapshot. IndexedDB is not a secure vault and should not hold secrets or
  sensitive records.

## Run MAUI

Keep PostgreSQL and the Web/API host running. In another terminal, run the
Windows MAUI target:

```powershell
dotnet run --project .\src\BlazorMultihostStarter.Maui `
  --framework net10.0-windows10.0.19041.0
```

`Maui:ApiBaseAddress` defaults to `http://localhost:5080` on Windows Debug and
`http://10.0.2.2:5080` on Android Debug. Android Debug permits local HTTP; a
Release build rejects a non-HTTPS API URL and keeps cleartext disabled.
Physical devices need a reachable development-machine address and firewall
access. MAUI sign-in uses MSAL and the platform broker/system browser.

## Run tests

```powershell
dotnet test .\tests\BlazorMultihostStarter.Domain.Tests\BlazorMultihostStarter.Domain.Tests.csproj
dotnet test .\tests\BlazorMultihostStarter.Application.Tests\BlazorMultihostStarter.Application.Tests.csproj
dotnet test .\tests\BlazorMultihostStarter.Api.IntegrationTests\BlazorMultihostStarter.Api.IntegrationTests.csproj
dotnet test .\tests\BlazorMultihostStarter.Shared.Tests\BlazorMultihostStarter.Shared.Tests.csproj
```

API integration tests default to SQLite in-memory and a test-only identity
scheme. CI also runs the API flow and migrations against PostgreSQL. Tests do
not contact a live Entra tenant or verify actual OIDC/MSAL redirects. To run
the PostgreSQL test path locally, set `TEST_POSTGRES_CONNECTION` to a
disposable database; never point tests at data that must be kept.
