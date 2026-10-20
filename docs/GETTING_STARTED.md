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
credentials. They are disposable local-development values, not production
secrets. Do not reuse them outside a local machine.

## Apply migrations and run Web/API

Run these commands from the repository root:

```powershell
dotnet tool restore
dotnet ef database update `
  --project .\src\BlazorMultihostStarter.Infrastructure\BlazorMultihostStarter.Infrastructure.csproj `
  --startup-project .\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj `
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext
dotnet run --project .\src\BlazorMultihostStarter.Web
```

The development HTTP profile listens on all interfaces at port `5080`; open
`http://localhost:5080` on the development machine. HTTPS can be enabled
locally with the launch profile's development certificate. The WebAssembly
client and API share the Web origin.

## Run MAUI

Leave PostgreSQL and the Web/API host running. For the Windows MAUI target:

```powershell
dotnet run --project .\src\BlazorMultihostStarter.Maui `
  --framework net10.0-windows10.0.19041.0
```

The local development API URL is currently configured in `MauiProgram.cs`.
Windows uses `http://localhost:5080`. Android Debug builds allow local HTTP
for development; Release builds keep Android's default cleartext protection.
Android emulators use
`http://10.0.2.2:5080`; physical devices need the development machine's
reachable LAN address and firewall access. Local HTTP is for development only.
For production, configure an HTTPS API endpoint and authentication before
distributing an app.

## Run tests

```powershell
dotnet test .\tests\BlazorMultihostStarter.Domain.Tests\BlazorMultihostStarter.Domain.Tests.csproj
dotnet test .\tests\BlazorMultihostStarter.Application.Tests\BlazorMultihostStarter.Application.Tests.csproj
dotnet test .\tests\BlazorMultihostStarter.Api.IntegrationTests\BlazorMultihostStarter.Api.IntegrationTests.csproj
dotnet test .\tests\BlazorMultihostStarter.Shared.Tests\BlazorMultihostStarter.Shared.Tests.csproj
```

API integration tests default to a disposable SQLite in-memory database.
To exercise the real PostgreSQL provider and migrations, set
`TEST_POSTGRES_CONNECTION` to a disposable database. CI uses a PostgreSQL
service for this test path.
