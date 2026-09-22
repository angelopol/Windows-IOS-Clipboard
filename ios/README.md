**English** · [Español](README.es.md)

# Clipboard — iPhone Shortcuts (iOS Shortcuts)

Three **Shortcuts** app shortcuts connect your iPhone to the shared clipboard.
There's no app to install: you build them by hand following the guides (signed
`.shortcut` files are fragile across iOS versions, so the reproducible guide is
what's reliable).

| Shortcut | What it does | Guide |
|----------|--------------|-------|
| **Send Clipboard** | Uploads the selection (share sheet) or the clipboard | [enviar-clipboard.md](enviar-clipboard.md) |
| **View Personal** | Last 5 texts uploaded **from this iPhone** (`scope=personal`) | [ver-clipboard.md](ver-clipboard.md) |
| **View Shared** | Last 5 from **all devices** (`scope=shared`) | [ver-compartido.md](ver-compartido.md) |

## Common requirements

- **Server URL** (e.g. `https://your-app.vercel.app`) and a **device token**
  created in the web panel (`/devices`). The same token works for all shortcuts.

## How to launch them (recommended)

- **Back Tap** (Settings → Accessibility → Touch → Back Tap): assign *Send* to a
  double tap and *View* to a triple tap, for example.
- **Action Button** (iPhone 15 Pro and later) for the one you use most.
- **Share sheet** on selected text (ideal for *Send* the selection and for
  *View* to replace the selection).

## Important iOS limitation

A shortcut **cannot** detect whether you're in a text field nor insert into
arbitrary apps. Because of that:

- *View Clipboard* **replaces the selection** only when launched from the share
  sheet with text selected; otherwise it **copies** to the clipboard and you
  paste.
- *Send Clipboard* uploads the **selection** if launched from share, or the
  **clipboard** if launched standalone.

## Personal vs. shared

- **Personal** (`GET /api/clips?scope=personal`) = only what was uploaded from
  **this device** (filtered by the token). Your own iPhone history.
- **Shared** (`GET /api/clips?scope=shared`, or no param) = the pool of **all**
  the account's devices. Same as what the Windows agent sees with `Ctrl+Alt+V`.

## API contracts

- `POST /api/clip` — `{ "text": "..." }` with `Authorization: Bearer <token>`.
- `GET /api/clips?scope=personal|shared` — returns `{ "clips": [...], "scope" }`
  (last 5). Without `scope`, defaults to `shared`.

Full detail in [`web/README.md`](../web/README.md).
