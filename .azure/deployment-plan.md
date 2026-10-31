# Optional Azure Showcase Plan

> **Status:** Planning — this is an optional, cost-gated portfolio path, not an
> approved deployment. No resources will be created until a separate cost,
> subscription, region, and quota review is approved.

Generated: 2026-10-31

## 1. Project Overview

**Goal:** The primary deliverable is a reusable `dotnet new` template for
future .NET applications. The checked-in Tasks application is its learning and
acceptance sample, not the product to deploy as a customer service. An
optional Azure showcase may validate how a freshly generated application is
deployed. The default path is private/local; Azure is not a requirement of the
generated base template.

**Path:** Modernize Existing

**Learning scope:** AI is an implementation assistant and tutor, not an AI
feature in the application. Each phase should explain decisions, alternatives,
failure paths, and verification.

## 2. Requirements

| Attribute | Value |
|---|---|
| Classification | Portfolio/demo with production-minded controls; not a customer SLA |
| Scale | Up to 10 users |
| Data | Synthetic/test data; EU preference |
| Distribution | Private installation; app stores are out of scope |
| Budget | Minimize recurring cost; estimate and obtain approval before provisioning |
| Subscription | Azure for Students is available, but its selected subscription, policies, services, remaining credit, and quotas have not been verified |
| Location | Prefer EU; exact Azure region is intentionally deferred until availability, policy, and cost are checked |
| Mobile targets | Windows, Android, iOS, and Mac Catalyst; Apple targets require macOS/Xcode build access |

Azure hosts the optional Web/API and database, not the MAUI client. MAUI builds
and runs on end-user devices. The current Microsoft platform guidance states
that iOS development requires a networked Mac; see
[supported .NET MAUI platforms](https://learn.microsoft.com/en-us/dotnet/maui/supported-platforms?view=net-maui-10.0).

## 3. Components Detected

| Component | Type | Technology | Path |
|---|---|---|---|
| Web/API | Web app and HTTP API in one host | ASP.NET Core .NET 10, Blazor Web, Entra ID | `src/BlazorMultihostStarter.Web` |
| Web client | Browser client | Blazor WebAssembly and MSAL | `src/BlazorMultihostStarter.Web.Client` |
| MAUI | Private native clients | .NET MAUI Blazor Hybrid and MSAL | `src/BlazorMultihostStarter.Maui` |
| Shared UI | Razor Class Library | Blazor components and IndexedDB modes | `src/BlazorMultihostStarter.Shared` |
| Backend | Domain/application/persistence | EF Core, Npgsql, PostgreSQL | `src/BlazorMultihostStarter.Domain`, `Application`, `Infrastructure` |
| Tests | Unit, API integration, shared UI | xUnit and bUnit | `tests` |
| CI | Build, test, template acceptance | GitHub Actions | `.github/workflows/ci.yml` |

**Existing Azure configuration:** no `azure.yaml`, Azure IaC, or application
Dockerfile is present. `docker-compose.yml` is for local PostgreSQL only.
There is no .NET Aspire AppHost.

## 4. Recipe Selection

**Preferred if the Azure showcase is activated:** Azure Developer CLI (AZD) +
Bicep.

**Rationale:** Azure is the preferred learning option because the application
already uses Microsoft Entra ID. Bicep teaches Azure-native infrastructure;
AZD can provide a repeatable application/environment workflow. This selection
is provisional until actual resource cost, student-subscription policies, and
regional support are checked. Local/private operation does not require AZD,
Bicep, or an Azure subscription.

## 5. Candidate Architecture

Keep the current single Web/API host unless a demonstrated requirement calls
for separate scaling or isolation. The first Azure candidate is:

| Component | Candidate Azure service | SKU/status |
|---|---|---|
| Web/API | Azure App Service for Linux | Select only after .NET 10, region, and cost validation |
| Relational database | Azure Database for PostgreSQL Flexible Server | Select compute, storage, backup, and networking only after estimate |
| Application telemetry | Application Insights, with its Log Analytics workspace | Optional; estimate ingestion and retention cost |
| Secret access | Managed Identity; Key Vault only for secrets that remain necessary | Scope least privilege; never publish credentials to clients |
| CI/CD identity | GitHub Actions OIDC federation to Entra | No long-lived Azure credentials in repository secrets |

The existing .NET host serves the Web app and API from one origin, so the
candidate avoids splitting the API or adding CORS without a requirement.
Before selecting the database connection method, validate the .NET/Npgsql
Managed Identity flow. Keep the existing manual migration boundary: deploy
schema changes as an explicit pipeline step, not automatically at app startup.

For the first small demo, assume one region and one application instance, with
synthetic data and no HA/SLA claim. Before scale-out, design distributed rate
limiting, shared ASP.NET Core Data Protection keys, and Interactive
Server/SignalR behavior. Private database networking, custom domains, slots,
and other paid resources require a documented cost/security reason.

## 6. Subscription, Cost, and Capacity Gate

No Azure resource inventory is currently authorized, so no subscription or
region has been selected and no quota/policy lookup has been performed.

Before activating this optional path:

1. Select the actual Azure for Students subscription and an EU region.
2. Verify subscription policies, service eligibility, available credit, quotas,
   and regional capacity.
3. Inventory the exact resources and quantities after architecture selection.
4. Estimate recurring and one-time costs, including database, compute,
   monitoring/log retention, network, and any domain.
5. Present the estimate and obtain the user's explicit spending approval.
6. Only then complete the quota checklist and proceed to Azure validation.

Microsoft's current [Azure for Students offer](https://azure.microsoft.com/en-us/free/students/)
is credit-limited and may change; verify the terms in the user's own
subscription before planning around any included credit. If a cost-effective
cloud footprint is not available, keep the portfolio project local/private
and defer Azure.

## 7. Roadmap and Approval

The implementation order and learning acceptance criteria are in
[ROADMAP.md](../ROADMAP.md), sections V4–V6:

1. Define the generated project's base contract and which examples/options are
   intentionally included.
2. Complete and test the production-minded baseline on a freshly generated
   project, not only on the template source solution.
3. Optionally add an ephemeral, cost-reviewed Azure showcase using AZD+Bicep.
4. Treat real customer deployments as a separate product-specific decision.

This draft is not Ready for Validation or deployment. Approval of the learning
roadmap does not approve Azure resource creation, paid services, subscription
changes, database migrations, or teardown. Each requires a separate explicit
approval.
