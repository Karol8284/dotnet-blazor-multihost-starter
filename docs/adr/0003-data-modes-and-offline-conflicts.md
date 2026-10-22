# ADR-0003: Data modes and offline conflict handling

- **Status:** accepted for the Tasks reference examples
- **Date:** 2026-10-22
- **Scope:** client-local, server-centric, and offline-hybrid task behavior

## Context

The starter is intended to teach when data belongs on a client, on a server, or
in a hybrid cache-and-sync flow. These choices affect consistency and
authorization and must not be represented by one ambiguous "offline" toggle.

## Decision

Provide three separate task routes:

| Route | Data mode | Data authority |
| --- | --- | --- |
| `/` | Server-centric | API/PostgreSQL |
| `/tasks/local` | Client-local | Current browser or MAUI WebView |
| `/tasks/offline` | Offline hybrid | Server after explicit synchronization |

Local and offline-hybrid state is stored in IndexedDB through a shared JS
adapter. Browser tabs share storage for the same origin; each installed app
WebView has its own storage partition. This reference store is for small,
low-sensitivity sample data only.

The offline-hybrid mode persists a snapshot and its last known server version.
Each sync request carries a stable task ID and `BaseVersion`. The server uses
optimistic concurrency and returns `409 Conflict` plus the current server
snapshot when `BaseVersion` is stale. The UI offers two explicit actions:

- **Keep server:** discard the conflicting local snapshot and adopt the server
  snapshot.
- **Keep local and retry:** rebase the local snapshot on the displayed server
  version and submit it again. If the server changes again, a new conflict is
  returned.

New tasks use client-generated GUIDs so retrying a create after a lost response
does not create a duplicate. Local writes remain on device if authentication
or the API is unavailable. Synchronization is manual; no background worker is
included.

## Consequences

- Server ownership and authorization remain server-side. A local record or
  client-provided task ID never determines the owner.
- The API version is a concurrency token in EF Core and is returned in
  `TaskResponse`.
- A local-only task is not shared with another device or account and is not
  automatically migrated to the server.
- The offline queue is a teaching example, not a general synchronization
  framework. It does not include deletes, multi-record transactions,
  background retry, encryption, or merge UI for arbitrary domain objects.
- Users should not store secrets or sensitive information in IndexedDB.

## Verification boundary

Unit/component tests verify local queue behavior, idempotent sync, stale-version
conflicts, and explicit resolution. API integration tests verify owner scoping
and `409` responses. Real browser/WebView IndexedDB persistence and a live
multi-device synchronization session still require manual platform smoke
tests.
