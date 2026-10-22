# ADR-0002: Authentication and client/server execution modes

- **Status:** Accepted for an Entra ID reference integration; app registration values remain deployment-specific
- **Date:** 2026-10-22
- **Scope:** the security baseline and architectural vocabulary for the Tasks reference flow

## Context

The starter has two UI hosts:

- a Blazor WebAssembly client hosted by ASP.NET Core;
- a .NET MAUI Blazor Hybrid application.

Both hosts can render the same Razor Class Library, but they do not have the
same runtime, storage, browser, or authentication capabilities. In particular,
the Web host, WebAssembly client, and MAUI client are separate security
boundaries.

The existing `Tasks` API is anonymous and stores all rows in one global list.
The feature is demo data today, but neither anonymous access nor global data
ownership is an acceptable default for a reusable starter.

## Separate two independent architecture choices

The starter distinguishes **where the UI executes** from **where application
operations and authoritative data live**.

### UI rendering/hosting

- **Interactive WebAssembly:** Razor UI event handling runs in the browser.
- **Interactive Server:** Razor UI event handling runs on the ASP.NET Core
  server over a persistent connection.
- **MAUI Blazor Hybrid:** Razor UI runs in the native app's WebView.

These are host/rendering choices. They do not make business rules or
authorization trustworthy when executed in a client.

### Operation and data authority

- **Client-local:** a feature uses device/browser-local state or storage and
  does not claim to be shared or server-authoritative. Suitable examples are
  presentation preferences and disposable drafts. Local storage is not a
  security boundary for secrets or shared records.
- **Server-centric:** the API and database are the source of truth. Clients
  may validate input for usability, but the API validates every request and
  authorizes every read and write.
- **Hybrid:** the client can cache server data or queue offline edits, but the
  server remains authoritative. Synchronization, conflict handling, replay,
  and ownership checks must be explicit; a client-side permission check never
  authorizes a server write.

These modes are selected per capability, not with one global runtime switch.
The `Tasks` module remains **server-centric** in this ADR. V1 does not implement
offline mutation queues or claim local-first synchronization.

## Authentication decision

The API trusts an external OpenID Connect (OIDC) identity provider. The starter
does not implement its own identity provider, password database, access-token
issuer, refresh-token protocol, or cryptographic signing keys.

The API accepts **OAuth access tokens** and validates their signature,
configured issuer, API audience, and lifetime using the provider's OIDC
metadata/signing keys. ID tokens are not API credentials. All Tasks endpoints
require an authenticated subject and issuer.

Both clients are public clients and must use Authorization Code with PKCE.
Client secrets must never be put in WebAssembly assets, MAUI packages, or
committed configuration. The browser and native clients use separate client
registrations and redirect URIs. They may share the API audience and required
delegated scope.

The API's data owner is derived only from the validated `(tid, oid)` claims.
For Entra, `oid` is stable across separate Web and MAUI app registrations,
while `sub` is a pairwise identifier and can differ by client ID. The API never
accepts a user/owner identifier from a request body, query parameter, or
client-controlled header. Every Tasks list, read, and mutation is scoped to
that owner. Existing anonymous demo rows receive a reserved legacy owner
during migration and remain inaccessible to authenticated users.

Integration tests use a test-only authentication handler that can issue
separate identities. It is registered only by the test host and is not a
development or production sign-in mechanism.

### Client authentication adapters

- WebAssembly uses the ASP.NET Core MSAL authentication integration with
  Authorization Code + PKCE and requests a delegated access token for the API.
- MAUI uses MSAL public-client flows and the platform broker/system browser.
  Windows uses the Windows broker; Android and iOS use their registered MSAL
  redirect configuration. The WebView is not used as an embedded
  credential-entry browser.
- Each host provides its own token-acquisition adapter to the shared API
  client. Shared Razor components do not know about provider SDKs or token
  storage.
- The selected reference provider is Microsoft Entra ID with MSAL. Tenant ID,
  API application ID, Web client ID, MAUI client ID, and redirect registrations
  are deployment-specific public configuration, not secrets. A different OIDC
  provider may be used by replacing the client adapters and API metadata
  configuration after its claims and platform flows are verified.
- The built-in .NET MAUI `WebAuthenticator` is documented as currently not
  working on Windows. The Windows adapter therefore uses MSAL's Windows
  broker rather than relying on `WebAuthenticator`.

## Consequences

- Anonymous access to `/api/tasks` is removed; unauthenticated requests receive
  `401`.
- User A cannot list, read, complete, or otherwise mutate User B's Tasks. An
  inaccessible task is returned as `404` to avoid disclosing its existence.
- The `Tasks` owner key is created at the API boundary from validated identity
  claims. Domain and persistence code receive that trusted owner key explicitly.
- Task listing is owner-scoped, uses a default page size of 25, enforces a
  maximum page size of 100, and returns whether another page exists.
- The Tasks API has an in-memory fixed-window rate limit of 60 requests per
  minute per authenticated owner. Multi-instance deployment needs a
  distributed limiter or an upstream gateway policy.
- WebAssembly and MAUI require different authentication registrations even
  though they reuse the same UI and API client abstraction.
- A tenant is not bundled by this ADR. Without a configured tenant and app
  registrations, the application must not silently fall back to anonymous
  access.
- Cookie/BFF authentication is not selected. If a future Web host uses
  authenticated cookies for API calls, add and test antiforgery protection for
  state-changing requests; `UseAntiforgery()` alone is not endpoint
  authorization.
- The `Tasks` reference flow is not production-ready until a real provider,
  HTTPS API addresses, production configuration, and both client sign-in
  paths have been exercised.

## Verification boundary

- Automated tests use a test-only authentication scheme. They verify
  anonymous rejection, required delegated scope, registered client IDs,
  pagination/rate limits, and cross-user task isolation.
- The production API uses `JwtBearer` metadata/signature, issuer, audience,
  expiry, Entra tenant, and `oid` validation. These real Entra token-validation
  and redirect flows are not exercised by the test scheme.
- A real Entra app registration and runtime sign-in test for Web and every
  claimed MAUI platform are required before declaring the authentication
  integration production-ready.
- Invalid or incomplete API configuration fails startup. Client IDs and scopes
  are public configuration; no client secret or token is included in source.

## Alternatives considered

### Copy the React starter's browser token flow

Rejected. The React application stores an access token in `localStorage`; this
is not a default to copy into a Blazor WebAssembly client. Browser and MAUI
clients have different token-storage mechanisms, and the API must remain the
authorization authority.

### Issue JWTs in this application

Rejected for the starter's first secure flow. Implementing an identity/token
issuer, password lifecycle, signing-key rotation, refresh-token replay
protection, account recovery, and multi-client protocols would be a separate
security product, not a small Tasks slice.

### Trust client-side authorization or a client-provided owner ID

Rejected. A caller can modify client code and request data. The API derives
the data owner from validated claims and scopes every data operation.

### Treat Interactive Server as server-side authorization

Rejected. Render mode is not a data-access policy. The API still validates
identity and ownership regardless of where a component happens to execute.

## References

- [Microsoft Learn: JWT bearer authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-10.0)
- [Microsoft Learn: Blazor WebAssembly OIDC authentication](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-10.0)
- [Microsoft Learn: .NET MAUI WebAuthenticator](https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/communication/authentication?view=net-maui-10.0)
- [Microsoft Learn: MAUI authentication and authorization](https://learn.microsoft.com/en-us/dotnet/architecture/maui/authentication-and-authorization)
