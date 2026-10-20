# Blazor Multihost Starter

A small .NET starter for a Blazor Web client and a .NET MAUI Blazor Hybrid app
that share one Razor UI library and call the same API.

**Status:** learning/reference starter, not production-ready software.

## What is included

- One solution with Web, MAUI, shared UI, API, persistence, and test projects.
- One `Tasks` module organized as vertical slices:
  - list tasks;
  - create a task;
  - complete a task.
- A single Razor Class Library (RCL) used by both WebAssembly and MAUI.
- PostgreSQL through EF Core. The Web host serves the Web client and the API;
  MAUI calls that same API.
- Domain, application, infrastructure, API contracts, and client code have
  explicit dependency boundaries.
- Unit, API integration, and shared-component tests.

The Web interactivity decision is recorded in
[ADR-0001](./docs/adr/0001-web-interactivity.md). The implementation uses
Interactive WebAssembly because the browser and MAUI app are both API clients.

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

From the repository root:

```powershell
docker compose up -d --wait postgres
dotnet ef database update `
  --project .\src\BlazorMultihostStarter.Infrastructure\BlazorMultihostStarter.Infrastructure.csproj `
  --startup-project .\src\BlazorMultihostStarter.Web\BlazorMultihostStarter.Web.csproj `
  --context BlazorMultihostStarter.Infrastructure.Data.StarterDbContext
dotnet run --project .\src\BlazorMultihostStarter.Web
```

Open `http://localhost:5080`. The development HTTP profile listens on all
interfaces so an Android emulator can reach the API; use the browser-facing
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

The shared Tasks screen uses the same API client in Web and MAUI. The default
development base address is set in `MauiProgram.cs`; for a physical device,
change it to an address reachable from that device. `localhost` on a device is
the device itself. See
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

## Architecture and adding features

- [Architecture](./docs/ARCHITECTURE.md) — projects, responsibilities, and dependency direction.
- [Adding a feature](./docs/ADDING_FEATURES.md) — the Tasks VSA golden path and test checklist.
- [Getting started](./docs/GETTING_STARTED.md) — local database, API URL, and MAUI targets.
- [Roadmap](./ROADMAP.md) — V1 scope and deliberate follow-up work.

This starter borrows architectural practices from
[dotnet-react-starter](https://github.com/Karol-Malecki-dev/dotnet-react-starter);
it intentionally does not copy that application's authentication, business
features, deployment stack, or operational tooling.