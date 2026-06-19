<!-- Maintained by spec-lite v0.0.8 | updated by: implement, fix sub-agents -->

# Feature Summary

> **Current state only.** This document reflects what each feature does *right now* — not what it used to do.
> Maintained by the Implement and Fix sub-agents after every code change that affects feature behavior.
> For change history, use source control (e.g., git).

---

## Real-Time Communication

**FEAT-FP-001: WebSocket & SSE Streaming Endpoints** *(updated: 2026-04-13 by implement)*
Exposes three real-time endpoints under `/api/ws/`. `GET /api/ws/echo` upgrades to a WebSocket and echoes every received frame back verbatim; plain HTTP requests return 400. `GET /api/ws/chat` upgrades to a WebSocket and routes text frames through `WebSocketChatService`, which returns canned keyword-matched replies (hello/help/joke/weather/time/name/bye) or a fallback quoting the original message; malformed JSON sends an error frame without closing the connection. `GET /api/ws/sse` is a regular HTTP endpoint that streams a message word-by-word in SSE format (`data: {"token":"…","index":N,"done":false}\n\n`) at a configurable `delayMs` delay (0–5000 ms), terminating with `data: [DONE]\n\n`; client disconnects are handled silently.

## Authentication & Authorization

**FEAT-AUTH-001: OAuth 2.0 Mock Authorization Code Flow** *(updated: 2026-06-13 by fix)*
Exposes OAuth simulation endpoints under `/api/auth/`. `GET /api/auth/authorize` validates `response_type=code`, `client_id`, and `redirect_uri`, then always issues a 302 redirect to the provided redirect URI with a generated `code` (and echoes `state` when provided) without requiring mock username/password query parameters. `GET /api/auth/callback` accepts redirect callbacks with or without `code`/`state` and returns a success payload confirming callback handling for mock OAuth handshake scenarios.

## Data APIs

**FEAT-DATA-001: Quick JSON Array Endpoint** *(updated: 2026-06-18 by fix)*
Exposes `GET /api/data/quick-array` to return a raw JSON array of three sample objects. Each object includes `id` (number), `name` (string), `category` (string), and `isActive` (boolean), enabling fast client-side JSON array/object parsing tests without requiring any data-store setup.
