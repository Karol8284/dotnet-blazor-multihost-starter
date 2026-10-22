# Getting started

## Requirements

- .NET SDK `10.0.402` or a later SDK in the .NET 10 feature band.
- Docker Desktop or a compatible Docker engine for local PostgreSQL.
- A MAUI workload and platform tooling for the host you want to run.

Check the SDK and workloads:

```powershell
dotnet --version
dotnet workload list
```

## Start PostgreSQL

Copy `.env.example` to `.env`, then start the local database:

```powershell
Copy-Item .\.env.example .\.env
docker compose up -d --wait postgres
```

The committed development connection string matches the sample Compose
credentials and connects to host port `5433` (the container listens on `5432`).
Port `5433` avoids conflicting with a PostgreSQL server already listening on
the host's default port. The credentials are disposable local-development
values, not production secrets. Do not reuse them outside a local machine.

## Apply migrations and run Web/API

Register an Entra API application with a delegated `Tasks.ReadWrite` scope,
plus Web SPA and MAUI public-client registrations. Grant both clients the
delegated API scope. Do not create client secrets for the public clients.

Set the API's Entra configuration in User Secrets (IDs are public values, but
keeping per-developer configuration out of tracked settings is convenient):

```powershell
$webProject = '.\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj'
dotnet user-secrets set 'Authentication:Entra:TenantId' '<tenant-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:ApiClientId' '<api-client-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:WebClientId' '<web-client-guid>' --project $webProject
dotnet user-secrets set 'Authentication:Entra:MauiClientId' '<maui-client-guid>' --project $webProject
```

Add the same public tenant/API IDs to
`src/BlazorMultihostStarter.Web.Client/wwwroot/appsettings.Development.json`
and `src/BlazorMultihostStarter.Maui/Resources/Raw/appsettings.json`, with the
appropriate public client ID and API scope for each client. The MAUI client ID
must also match `MauiEntraRegistration.ClientId` in
`src/BlazorMultihostStarter.Maui/Authentication/MauiEntraOptions.cs`. Register
the Web redirect `http://localhost:5080/authentication/login-callback`, the
Android redirect `msal<MAUI_CLIENT_ID>://auth`, the iOS redirect
`msauth.com.example.blazormultihoststarter://auth`, and the Windows broker
redirect `ms-appx-web://microsoft.aad.brokerplugin/<MAUI_CLIENT_ID>`. Match
the Android intent filter and iOS/Mac Catalyst URL schemes in the platform
files to the registered MAUI client ID and bundle ID.

Run the remaining commands from the repository root:

```powershell
dotnet tool restore
dotnet ef database update `
  --project .\src\BlazorMultihostStarter.Infrastructure\BlazorMultihostStarter.Infrastructure.csproj `
  --startup-project .\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj `
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext
dotnet run --project .\src\BlazorMultihostStarter.Web
```

The development HTTP profile listens on all interfaces at port `5080`; open
`http://localhost:5080` on the development machine. The browser client and API
share the Web origin. The API requires a valid Entra bearer token and rejects
anonymous requests. Production configuration must use the deployment's
HTTPS authority, API origin, audience, client IDs, and redirect URIs.
Outside Development, startup also rejects wildcard `AllowedHosts` and the
sample PostgreSQL password; provide explicit host names and a managed database
connection string through the deployment environment.

## Run MAUI

Leave PostgreSQL and the Web/API host running. For the Windows MAUI target:

```powershell
dotnet run --project .\src\BlazorMultihostStarter.Maui `
  --framework net10.0-windows10.0.19041.0
```

`Maui:ApiBaseAddress` defaults to `http://localhost:5080` on Windows Debug and
`http://10.0.2.2:5080` on Android Debug. Android Debug permits local HTTP; a
Release build rejects a non-HTTPS API URL and keeps cleartext disabled. Physical
devices need the development machine's reachable LAN address and firewall
access. MAUI sign-in uses MSAL and the platform broker/system browser.

## Run tests

```powershell
dotnet test .\tests\BlazorMultihostStarter.Domain.Tests\BlazorMultihostStarter.Domain.Tests.csproj
dotnet test .\tests\BlazorMultihostStarter.Application.Tests\BlazorMultihostStarter.Application.Tests.csproj
dotnet test .\tests\BlazorMultihostStarter.Api.IntegrationTests\BlazorMultihostStarter.Api.IntegrationTests.csproj
dotnet test .\tests\BlazorMultihostStarter.Shared.Tests\BlazorMultihostStarter.Shared.Tests.csproj
```

API integration tests default to a disposable SQLite in-memory database and
use a test-only identity scheme; they do not test a live Entra tenant. To
exercise the real PostgreSQL provider and migrations, set
`TEST_POSTGRES_CONNECTION` to a disposable database. CI uses a PostgreSQL
service for this test path.
