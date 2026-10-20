# Adding a feature

Use the Tasks module as the VSA reference. Start with one user-visible use
case, then add only the code and tests that use case needs.

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
6. Add the operation to `ITasksApiClient` and its implementation if a UI host
   needs it.
7. Add or update the feature UI in
   `Shared/Modules/Tasks/`. Host-specific behavior belongs in host services
   injected through DI, not `#if` branches in UI.

## Tests to add

- Domain: the invariant and its invalid boundary cases.
- Application: success, not-found/validation outcomes, and correct port calls.
- API integration: status code, response contract, persistence, and invalid
  input. Run against PostgreSQL in CI or with `TEST_POSTGRES_CONNECTION`; the
  local default is SQLite in-memory.
- Shared UI: loading, empty, success, validation, and API-failure states when
  the component changes.
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
