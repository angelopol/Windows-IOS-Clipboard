**English** · [Español](ver-compartido.es.md)

# iOS Shortcut — "View Shared" (last 5 from all devices)

Shortcut to **view the shared clipboard**: the last 5 texts uploaded from **any**
device on your account (Windows, iPhone, others). Shows them in a menu and copies
the one you pick. To see only your own from this iPhone, use
[View Personal](ver-clipboard.md).

It's identical to "View Personal" except for the URL (without `scope=personal`,
or with `scope=shared`).

---

## Requirements

- **Server URL** — e.g. `https://your-app.vercel.app`
- **Device token** — from the web panel (`/devices`). The same one the other
  shortcuts use.

---

## Endpoint contract

**Request**
```
GET /api/clips?scope=shared
Authorization: Bearer <DEVICE_TOKEN>
Accept: application/json
```
> `scope=shared` is the default: calling `/api/clips` without a param returns the
> same thing.

**Response `200`**
```json
{
  "clips": [
    { "id": 43, "text": "copied on the PC", "created_at": "2026-09-21T18:05:00Z" },
    { "id": 42, "text": "copied on the iPhone", "created_at": "2026-09-21T18:00:00Z" }
  ],
  "scope": "shared"
}
```
- Order: **most recent first**. Max 5.
- Errors: `401` if the token is invalid/revoked.

---

## How to build the shortcut (Shortcuts app)

1. Open **Shortcuts** → **+** (new shortcut). Name it `View Shared`.
2. **Text** → the server base URL, e.g. `https://your-app.vercel.app`.
3. **Get Contents of URL**:
   - **URL**: the variable from step 2 + `/api/clips?scope=shared` → becomes
     `https://your-app.vercel.app/api/clips?scope=shared`.
   - **Show more** → **Method**: `GET`.
   - **Headers**:
     - `Authorization` = `Bearer YOUR_TOKEN`
     - `Accept` = `application/json`
4. **Get Dictionary Value** → key `clips` (gives you the list of 5 objects).
5. **Get Dictionary Value** again → key `text`, **input: the whole list from
   step 4** (not a single item — don't use "Repeat with Each" here). Shortcuts
   returns a flat list of exactly 5 texts. See Troubleshooting in
   [ver-clipboard.md](ver-clipboard.md#troubleshooting-i-get-more-than-5-items--raw-data-instead-of-clean-text)
   if you see more than 5 or raw records instead of clean text.
6. **Choose from List** → list = the **output of step 5** (not step 4's list of
   `{id, text, created_at}` objects), prompt `Choose a text`.
7. **Copy to Clipboard** → result of step 6.
8. *(Optional)* **Show Notification**: "Copied ✓".

### "Replace selection" variant
Same as in [View Personal](ver-clipboard.md): enable **Show in Share Sheet**
accepting **Text**, and leave the chosen text as the shortcut's **output**
(without "Copy to Clipboard") to replace the selection when launched from the
share sheet.

---

## How to launch it

- **Action Button / Back Tap / widget / home**: "view and copy" mode.
- **Share sheet** on selected text: "replace selection" mode.

## Notes

- This is the iOS equivalent of the Windows agent's `Ctrl+Alt+V` menu: both read
  the same shared pool.
- The token travels in the header, always over **HTTPS**.
- If you get `401`, generate a new token at `/devices`.
