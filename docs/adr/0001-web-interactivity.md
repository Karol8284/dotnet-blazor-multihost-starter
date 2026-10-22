# ADR-0001: Web interactivity

- **Status:** accepted; Interactive WebAssembly is the default, Interactive Server is an opt-in host mode
- **Date:** 2026-10-20; updated 2026-10-22
- **Scope:** Web rendering and its host-specific authentication/client wiring

## Context

The solution has a browser client, a .NET MAUI Blazor Hybrid client, one shared
Razor Class Library, and a protected Tasks API. Web and MAUI share components,
but their runtime and authentication boundaries differ. Web rendering mode
must not be confused with where task data is authoritative.

## Options

### Interactive Server

- Browser UI events execute on the ASP.NET Core server over a persistent
  SignalR circuit.
- It avoids downloading the WebAssembly runtime and client assemblies.
- Interactivity requires a live connection and server circuit.
- The Web host uses an Entra OIDC confidential client and cookie session.
  Server-side UI calls Application handlers directly under the validated
  principal; MAUI continues to call the bearer-protected API.

### Interactive WebAssembly

- UI events execute in the browser after the runtime and client assemblies load.
- The browser and MAUI are both API clients, which keeps their data access
  boundary similar.
- The WebAssembly public client uses MSAL Authorization Code + PKCE and calls
  the same-origin API with a delegated access token.
- The initial payload is larger than Interactive Server. Co-hosting Web assets
  and the API avoids CORS in the default deployment shape.

## Decision

Support both Interactive WebAssembly and Interactive Server in the Web host.
`Web:Interactivity` selects the mode; WebAssembly is the default. Keep common
UI in the RCL. Keep authentication and `ITasksClient` registrations in each
host's composition root.

The mode is selected per Web host deployment, not per component. MAUI always
uses Blazor Hybrid. Data authority remains a separate per-feature decision.

## Consequences

- The Web host registers and maps both interactive render modes.
- In WebAssembly mode, the Web.Client assembly uses MSAL and the protected
  HTTP API.
- In Server mode, the Web host uses OIDC/cookie authentication and an
  application-layer adapter; it does not send a browser token to its own API.
- Server mode requires a confidential Entra client and a secret supplied
  through User Secrets or deployment secret storage. WebAssembly and MAUI are
  public clients and must not contain a client secret.
- Both modes share RCL components. They do not have identical request paths,
  so host-specific differences belong in DI adapters rather than Razor
  `#if` branches.
- Interactive Auto and mixed per-component render modes are outside this
  starter's scope.

## Verification boundary

- CI and integration tests verify that the Web host can serve each configured
  render-mode shell and that the API policies work with a test identity.
- The real Entra confidential-client sign-in, SPA redirect, MAUI broker flow,
  and sign-out behavior are not verified by these tests.
- No performance or bundle-size benchmark was collected. The default is chosen
  for the browser-plus-MAUI API-client model, not from a benchmark.

## References

- [Microsoft Learn: Blazor render modes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes?view=aspnetcore-10.0)
- [Microsoft Learn: Blazor WebAssembly OIDC authentication](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/standalone-with-authentication-library?view=aspnetcore-10.0)
