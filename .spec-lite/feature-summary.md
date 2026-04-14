<!-- Maintained by spec-lite v0.0.8 | updated by: implement, fix sub-agents -->

# Feature Summary

> **Current state only.** This document reflects what each feature does *right now* — not what it used to do.
> Maintained by the Implement and Fix sub-agents after every code change that affects feature behavior.
> For change history, use source control (e.g., git).

---

## Real-Time Communication

**FEAT-FP-001: WebSocket & SSE Streaming Endpoints** *(updated: 2026-04-13 by implement)*
Exposes three real-time endpoints under `/api/ws/`. `GET /api/ws/echo` upgrades to a WebSocket and echoes every received frame back verbatim; plain HTTP requests return 400. `GET /api/ws/chat` upgrades to a WebSocket and routes text frames through `WebSocketChatService`, which returns canned keyword-matched replies (hello/help/joke/weather/time/name/bye) or a fallback quoting the original message; malformed JSON sends an error frame without closing the connection. `GET /api/ws/sse` is a regular HTTP endpoint that streams a message word-by-word in SSE format (`data: {"token":"…","index":N,"done":false}\n\n`) at a configurable `delayMs` delay (0–5000 ms), terminating with `data: [DONE]\n\n`; client disconnects are handled silently.
