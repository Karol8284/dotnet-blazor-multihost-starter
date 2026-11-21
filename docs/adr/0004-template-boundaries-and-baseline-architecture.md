# ADR-0004: Template boundaries and baseline architecture

- **Status:** accepted
- **Date:** 2026-10-31
- **Scope:** generated project baseline, modularity of sample slices, and starting a new application

## Context

The primary deliverable of this repository is an installable `dotnet new` template
used to bootstrap production-minded, multi-host .NET applications sharing a single
Razor Class Library (RCL). The reference repository includes both the foundational
hosting/authentication platform and a concrete sample module (`Tasks`).

Without explicit boundaries, developers generating a new project may treat the
demonstration domain (`Tasks`) as mandatory scaffolding, or struggle to safely
substitute their own domain. Additionally, adding speculative configuration
options to the template generator creates maintenance overhead without proven demand.

## Decision

### 1. Invariant platform baseline (always included)

The base generated solution contains the opinionated multi-host foundation:

- **Hosting & UI:**
  - Blazor Web host supporting both Interactive WebAssembly (default) and
    Interactive Server via `Web:Interactivity` configuration.
  - .NET MAUI client hosting Blazor Hybrid with native platform shell.
  - Shared Razor Class Library (RCL) holding UI components, layouts, and styles.
- **Backend & Persistence:**
  - ASP.NET Core minimal API with vertical slices.
  - PostgreSQL persistence with Entity Framework Core and explicit migrations
    applied via `dotnet ef database update` (never automatic at startup in production).
- **Security & Identity:**
  - Microsoft Entra ID with MSAL (PKCE) for public clients (WebAssembly and MAUI)
    and OIDC/cookie session for Interactive Server.
  - Fail-closed security baseline: unauthenticated requests to protected APIs are
    rejected (`401`), and owner identity is derived solely from verified `(tid, oid)`
    claims, never from client-controlled request bodies or headers.
  - Client secrets and server connection strings are strictly kept out of clients,
    template parameters, and source code.
- **Clean dependency boundaries:**
  - Dependencies point inward: `Domain` has no dependencies; `Contracts` defines
    DTOs; `Application` depends on `Domain`; `Infrastructure` implements application
    ports; `Client` depends on `Contracts`; `Shared` depends on `Client` and `Contracts`.

### 2. Sample vertical slice (`Tasks`) boundary

The `Tasks` module is a self-contained, reference vertical slice designed to
demonstrate the three data modes described in ADR-0003:
- Server-centric (`/`): API and PostgreSQL are authoritative.
- Client-local (`/tasks/local`): IndexedDB on the current browser or app WebView.
- Offline hybrid (`/tasks/offline`): local persistence with optimistic concurrency
  versioning and explicit conflict resolution.

`Tasks` is **not** a required domain for generated applications. It is located in
isolated module folders:
- `Domain/Tasks/`
- `Application/Modules/Tasks/`
- `Infrastructure/Modules/Tasks/`
- `Client/Tasks/`
- `Shared/Modules/Tasks/`
- `Web/Modules/Tasks/`

### 3. How to start a new application from the generated template

When starting a new application:
1. **Model the domain:** Define new domain entities and invariants in `Domain/`.
2. **Add vertical slices:** Implement use cases in `Application/Modules/<Feature>/`,
   persistence adapters in `Infrastructure/Modules/<Feature>/`, contracts in
   `Contracts/<Feature>/`, client methods in `Client/<Feature>/`, and UI components
   in `Shared/Modules/<Feature>/` as outlined in `docs/ADDING_FEATURES.md`.
3. **Update navigation:** Update `Shared/Layout/NavMenu.razor` to route to the new
   application features.
4. **Retire or adapt the sample:**
   - Keep `Tasks` initially as a reference for data modes, auth tokens, and EF mappings.
   - Once the new feature is verified, delete the `Tasks` module folders and create a
     fresh EF migration for the target schema.

### 4. Template parameters and options

The template generator provides only parameters with verified, repetitive use:
- `--name`: Solution and project naming (supports PascalCase, kebab-case, and dotted notation).
- `--appTitle`: Human-readable title for UI and app headers.
- `--mauiBundleId`: Reverse-DNS identifier for MAUI and native platform callbacks.
- Public Entra GUIDs: `--tenantId`, `--apiClientId`, `--webClientId`, `--mauiClientId`, `--serverClientId`.

Speculative options (e.g. conditional database providers, alternative auth stacks,
or modular microservice flags) are deliberately omitted. Adding template options
requires a demonstrated use case and dedicated CI test coverage.

## Consequences

- Generated applications have an immediate, working end-to-end golden path.
- The separation between foundational hosting and domain slices is unambiguous.
- Removing or replacing the sample domain slice does not damage the underlying
  multi-host authentication, routing, or persistence infrastructure.
- Template maintenance remains manageable because generator complexity is kept low.

## Verification boundary

- CI validates clean template generation for PascalCase, kebab-case, and dotted names
  in an isolated hive.
- CI verifies that generated projects build, reference all expected projects, contain
  no unresolved placeholders, and do not leak server secrets into client artifacts.

## References

- [ADR-0001: Web interactivity](./0001-web-interactivity.md)
- [ADR-0002: Authentication and execution modes](./0002-authentication-and-execution-modes.md)
- [ADR-0003: Data modes and offline conflict handling](./0003-data-modes-and-offline-conflicts.md)
- [Adding a feature](../ADDING_FEATURES.md)
