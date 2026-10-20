# ADR-0001: Blazor Web interactivity

- **Status:** accepted for V1
- **Date:** 2026-10-20
- **Decision:** Interactive WebAssembly

## Context

The starter has two UI hosts: a browser client and a .NET MAUI Blazor Hybrid
client. Both must reuse the same Razor Class Library (RCL), and both call the
same HTTP API. The Web host serves the WebAssembly client and API from one
origin for local development and the initial deployment shape.

The official `maui-blazor-web` template generates a shared RCL for both
interactive modes. Its WebAssembly variant has a Web server project and a
separate `.Web.Client` project; its Server variant runs interactive components
on the server and does not need that client project. Template generation
confirmed those project shapes. This was a structural comparison, not a
performance benchmark.

## Options

### Interactive Server

- The browser sends UI events over a persistent SignalR connection; component
  code runs on the server.
- It avoids downloading the .NET client runtime and application assemblies.
- It requires live server circuits and a reachable server connection for
  interactivity.
- The browser UI is a server-rendered client, while MAUI runs components
  locally in its WebView. That gives the two hosts different runtime
  boundaries.

### Interactive WebAssembly

- Interactive components and their event handlers run in the browser.
- The browser and MAUI clients both call the same HTTP API, matching the
  intended host boundary.
- It downloads the WebAssembly runtime and client assemblies before
  interactivity, so initial download size is larger.
- WebAssembly cannot access server-only services. API calls, CORS, and
  same-origin hosting must be configured intentionally.
- Co-hosting the Web client and API avoids CORS for the initial Web deployment.

## Decision

Use **Interactive WebAssembly** for the Web host in V1. Keep reusable UI in the
RCL, and keep API access behind a client-side service that can be registered by
both the WebAssembly and MAUI hosts. The Web host may serve both static Web
assets and API endpoints, but the browser client must not reference server
Infrastructure or the database.

## Consequences

- The solution contains a Web server project and a Web client project in
  addition to the MAUI and shared UI projects.
- Web and MAUI use the same API client abstraction and public contracts; each
  host configures its own `HttpClient` base address.
- The Web host serves API and Web assets from one origin in the default setup.
  A separately hosted API would require explicit CORS and deployment
  configuration.
- WebAssembly prerendering is disabled for the shared UI. The server doesn't
  register a server-side `ITasksApiClient`; data fetching starts after the
  WebAssembly client boots, so the Web host doesn't make an HTTP request back
  into itself while rendering the initial page. The first interactive content
  appears after the WebAssembly bundle loads.
- This choice is not a claim that WebAssembly is faster or smaller. If the
  starter later needs server-side rendering, reduced initial download, or
  another deployment topology, record and test that change in a new ADR.
- Interactive Auto and mixed per-component render modes remain out of V1.

## Validation

- The official template generated both Server and WebAssembly variants with a
  shared RCL.
- The WebAssembly template produced a distinct `.Web.Client` project; the
  Server variant did not.
- No latency or bundle-size measurements were collected. The choice is based
  on the two-client/API requirement, not a benchmark.
