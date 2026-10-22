# ADR-0002: Authentication and execution modes

- **Status:** accepted for the Entra/PostgreSQL reference starter
- **Date:** 2026-10-22
- **Scope:** host authentication boundaries, data authority, and task ownership

## Context

The solution has WebAssembly, Interactive Server, and MAUI Blazor Hybrid
execution paths. A shared Razor UI does not make those hosts the same security
boundary. The original Tasks demo was anonymous and stored every row in one
global list; that must not be the default for server-owned data.

Rendering mode and data authority are independent choices:

- Web rendering: Interactive WebAssembly or Interactive Server.
- Data authority per feature: server-centric, client-local, or offline hybrid.

## Authentication decision

The reference identity provider is Microsoft Entra ID. The starter does not
issue tokens, manage passwords, or implement its own identity provider.

- The API accepts OAuth access tokens, validates issuer, audience, lifetime,
  signature, allowed client, delegated scope, tenant, and object ID.
- WebAssembly and MAUI are public clients and use MSAL Authorization Code +
  PKCE with separate client IDs and redirect registrations. No client secret
  is shipped in either client.
- Interactive Server uses an OIDC confidential Web client and an HttpOnly
  cookie session. Its server-side task adapter invokes the Application
  handlers directly under the validated principal; it does not forward a
  browser token to the API.
- The API owner is derived only from validated `(tid, oid)` claims. The API
  never accepts a user or owner identifier from a request body, query, or
  client-controlled header. Reads and mutations always include that owner.
- Integration tests use a test-only identity handler. It is not a development
  sign-in mechanism and cannot replace real Entra validation.

Microsoft Entra ID and PostgreSQL are opinionated reference providers. Another
identity provider or database is not a configuration-only switch; its claim,
token, persistence, migration, and platform behavior must be adapted and
verified separately.

## Data mode decisions

### Server-centric

The API and PostgreSQL are authoritative. The API verifies the bearer token,
delegated scope, client application, and owner before using a task handler.
The Interactive Server Web host applies the same owner derivation before
invoking Application handlers directly. MAUI and WebAssembly call the API.

### Client-local

The `/tasks/local` example stores low-sensitivity task data in IndexedDB for
the current browser or app WebView. It has no API, cross-device sharing, or
sign-in requirement. IndexedDB is not a secure vault and must not store
credentials or sensitive records.

### Offline hybrid

The `/tasks/offline` example saves local changes and a pending snapshot in
IndexedDB before sync. The server remains authoritative. Every sync request
contains the client-generated task ID and the last known `BaseVersion`; it
does not contain an owner ID.

The server applies a snapshot only when its base version matches. New client
IDs make create retries idempotent. A stale version returns `409 Conflict`
with the current server snapshot. The user chooses either to keep the server
snapshot or rebase and retry the local snapshot. The example has no background
sync worker and does not silently use last-write-wins.

## Consequences

- Every API Tasks route requires authentication and the `Tasks.ReadWrite`
  delegated scope. Inaccessible records return `404` to avoid disclosing
  whether another user owns them.
- List requests default to 25 items and are capped at 100. The process-local
  rate limit is 60 requests per owner per minute; scale-out needs a distributed
  limiter or gateway policy.
- EF uses a task version concurrency token. Concurrent writes are reported as
  conflicts rather than overwriting a newer snapshot.
- Web interactivity is selected by `Web:Interactivity`; data modes are separate
  route examples and are selected per feature, not by one global data switch.
- Production configuration requires verified HTTPS redirects and host names.
  Interactive Server's client secret belongs in User Secrets or a secret
  manager.

## Verification boundary

- Automated tests cover anonymous rejection, scope/client checks, owner
  isolation, pagination/rate limits, sync retry behavior, and stale-version
  conflicts. CI exercises the API/migration path against PostgreSQL.
- Tests use a test identity. They do not verify real Entra metadata/token
  validation, browser redirects, native broker callbacks, or sign-out on
  physical devices.
- The local and hybrid pages use IndexedDB through JS interop. Host builds and
  unit/component tests do not replace runtime storage tests on every target.
- Do not describe the authentication flow as production-ready until real
  tenant registrations and host-specific sign-in paths have been exercised.

## Alternatives considered

### Trust client-side authorization or an owner ID in the request

Rejected. A client can be modified; the server derives ownership from a
validated identity.

### Copy a browser token-storage flow into MAUI

Rejected. WebAssembly and native clients have different storage and callback
requirements. Both use host-specific MSAL adapters.

### Automatically resolve stale offline writes with last-write-wins

Rejected. It can silently lose user edits. The sample reports a conflict and
requires an explicit choice.

### Claim local-first synchronization for the client-local mode

Rejected. `/tasks/local` is device-local only. Cross-device synchronization is
demonstrated separately by the offline-hybrid route.

## References

- [Microsoft Learn: JWT bearer authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0)
- [Microsoft Learn: Blazor WebAssembly OIDC authentication](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-10.0)
- [Microsoft Learn: MAUI authentication and authorization](https://learn.microsoft.com/en-us/dotnet/architecture/maui/authentication-and-authorization)
