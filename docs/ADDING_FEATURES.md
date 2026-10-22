# Adding a feature

Use the Tasks module as the VSA reference. Start with one user-visible use
case, then add only the code and tests that use case needs.

Before implementing a feature, choose its data mode:

- server-centric: API and database are authoritative;
- client-local: data stays in the current browser/device and is not synced;
- offline hybrid: persist locally, then synchronize against the server with an
  explicit conflict policy.

Choose the Web runtime separately with `Web:Interactivity` (`WebAssembly` or
`Server`). Do not treat rendering mode as authorization or data authority.

## Add a use case to Tasks

For a new operation:

1. Decide which invariant or outcome the user needs. Put business invariants
   in `Domain`, not in Razor components or EF configuration.
2. Add an application slice under
   `Application/Modules/Tasks/<UseCase>/` with a small request/result and
   handler. Inject only the ports that use case needs.
3. Add or reuse a persistence port in the Tasks module. Implement it in
   `Infrastructure/Modules/Tasks/`.
4. Add a public request/response contract only if the HTTP API needs it.
5. Add an endpoint in the Tasks API module. Validate external input and map
   expected validation and not-found results to explicit HTTP responses.
   Require the Tasks authorization policy and derive owner identity only from
   validated claims; never accept an owner ID from a client request.
6. For server-centric work, add the operation to `ITasksClient` and its HTTP
   implementation. The Interactive Server host has a server-side adapter to
   Application handlers; it must derive the same owner from the authenticated
   principal. Do not put Infrastructure in the shared UI.
7. For client-local or offline work, use `ILocalTasksClient` or
   `IHybridTasksClient` and persist through `ITasksLocalStore`. Keep
   local-only data separate from the server queue.
8. For sync operations, define retry/idempotency behavior, a version or other
   concurrency token, and explicit conflict responses. Never silently
   overwrite newer server state.
9. Add or update the feature UI in
   `Shared/Modules/Tasks/`. Host-specific behavior belongs in host services
   injected through DI, not `#if` branches in UI.

## Tests to add

- Domain: the invariant and its invalid boundary cases.
- Application: success, not-found/validation outcomes, and correct port calls.
- API integration: status code, response contract, persistence, and invalid
  input, anonymous rejection, and cross-user access denial. Run against
  PostgreSQL in CI or with `TEST_POSTGRES_CONNECTION`; the local default is
  SQLite in-memory with test-only identities.
- Collection endpoints must define a maximum page size and an ownership-aware
  query. Do not return an unbounded set of a user's rows.
- Client requests obtain a delegated access token through the host's
  `IUserAuthenticationSession`; never pass bearer tokens through Razor
  component parameters, URLs, or logs.
- Shared UI: loading, empty, success, validation, and API-failure states when
  the component changes.
- Client-local: storage and validation behavior without network access.
- Offline hybrid: local persistence while offline, sync retry, stale-version
  conflict, and both explicit resolution choices.
- Build the Web host and a supported MAUI target after changing shared code.

## Add a new module

Create a new module only when a distinct capability needs its own rules or
data. Keep dependencies pointed inward and use an explicit module entry point
for API mapping and infrastructure registration. Do not share EF entities
across modules or move code into a generic `Shared` project just because two
files look similar.

Before introducing a mediator, message bus, generic repository, or source
generator, identify repeated code in several implemented use cases and record
the concrete benefit in an ADR.
