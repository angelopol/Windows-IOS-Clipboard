**English** · [Español](README.es.md)

# Cross-platform Clipboard (Windows ↔ iPhone)

A clipboard shared across devices through a central server. Copy something on
one device and paste it on another: your account's latest texts are available on
both Windows and iPhone.

## How it works

- **Server (Vercel)** keeps a short list (last ~20, shows 5) per account. There
  is no real-time sync: **push on copy + pull on demand**, which fits serverless.
- **Windows**: a background agent uploads to the server every time you copy, and
  `Ctrl+Alt+V` shows the shared texts to paste. *(Phase 3)*
- **iPhone**: Shortcuts app shortcuts to send (clipboard/selection) and view the
  last 5. *(Phase 2)*
- **Web**: login and a panel to link/revoke devices via tokens.

```
  iPhone (Shortcuts)          Windows (agent + tray)
        │  ▲                          │  ▲
   POST │  │ GET                 POST │  │ GET (Ctrl+Alt+V)
        ▼  │                          ▼  │
   ┌─────────────────────────────────────────┐
   │   Vercel — Next.js  (/web)               │
   │   /api/clip  /api/clips  /api/devices    │
   └───────────────────┬─────────────────────┘
                        ▼
                    Neon Postgres
```

## Repo layout

| Folder | What it is | Status |
|--------|------------|--------|
| [`web/`](web/README.md) | API + auth panel (Next.js + Neon, Vercel) | ✅ Phase 0 + 1 |
| [`ios/`](ios/README.md) | Shortcuts + guides | ✅ Phase 2 |
| [`agent-win-cs/`](agent-win-cs/README.md) | Windows agent (C# / WPF, Win11-style flyout) | ✅ current |
| [`agent-win/`](agent-win/README.md) | Windows agent (Python + tray) | 🗄️ legacy |

## Roadmap

- [x] **Phase 0** — API `/api/clip` + `/api/clips`, DB (Neon), token auth
- [x] **Phase 1** — Web: login/registration + device panel
- [x] **Phase 2** — iPhone shortcuts: [send](ios/enviar-clipboard.md) + [view](ios/ver-clipboard.md)
- [x] **Phase 3** — Windows agent (auto-push on copy + `Ctrl+Alt+V` to paste)
- [x] **Phase 4** — Polish: dedup (server) + offline retries (agent). E2E
  documented as not viable with Shortcuts → see [`SECURITY.md`](SECURITY.md)

## Getting started

See [`web/README.md`](web/README.md) to run the server and deploy to Vercel.
Then, on `/devices`, generate a per-device token and use it in the iOS shortcut
([`ios/ver-clipboard.md`](ios/ver-clipboard.md)) or in the Windows agent.

## Security

- Revocable per-device tokens, stored as SHA-256 hashes.
- Passwords with bcrypt, session in an httpOnly cookie (JWT).
- Everything over HTTPS. E2E encryption is not viable with iOS Shortcuts; detail
  and a future path in [`SECURITY.md`](SECURITY.md).
