**English** · [Español](ver-clipboard.es.md)

# iOS Shortcut — "View Personal" (last 5 from this iPhone)

Shortcut to **view your personal clipboard**: asks the server for the last 5
texts you uploaded **from this iPhone**, shows them in a menu and copies the one
you pick. To see texts from all your devices, use
[View Shared](ver-compartido.md).

---

## Requirements

- **Server URL** — e.g. `https://your-app.vercel.app`
- **Device token** — generated in the web panel (`/devices`) and pasted into the
  shortcut. It's secret: don't share it.

> The shortcut uses `GET /api/clips?scope=personal`. `scope=personal` filters by
> the token's device, so you only see what was uploaded from this iPhone.

---

## Endpoint contract used by this shortcut

**Request**
```
GET /api/clips?scope=personal
Authorization: Bearer <DEVICE_TOKEN>
Accept: application/json
```

**Response `200`**
```json
{
  "clips": [
    { "id": 42, "text": "first text (the most recent)", "created_at": "2026-09-21T18:00:00Z" },
    { "id": 41, "text": "second text", "created_at": "2026-09-21T17:55:00Z" },
    { "id": 40, "text": "third", "created_at": "2026-09-21T17:40:00Z" },
    { "id": 39, "text": "fourth", "created_at": "2026-09-21T17:10:00Z" },
    { "id": 38, "text": "fifth", "created_at": "2026-09-21T16:30:00Z" }
  ]
}
```
- Order: **most recent first**. Max 5.
- The response includes `"scope": "personal"`.
- Errors: `401` if the token is invalid/revoked.

---

## How to build the shortcut (Shortcuts app)

1. Open **Shortcuts** → **+** (new shortcut). Name it `View Personal`.
2. **Text** → type the server base URL, e.g. `https://your-app.vercel.app`.
   (Storing it in a Text action makes it easy to change later.)
3. **Get Contents of URL**:
   - **URL**: the variable from step 2 + `/api/clips?scope=personal` → becomes
     `https://your-app.vercel.app/api/clips?scope=personal`.
   - Tap **Show more** → **Method**: `GET`.
   - **Headers** → add:
     - `Authorization` = `Bearer YOUR_TOKEN`  *(paste your token here)*
     - `Accept` = `application/json`
4. **Get Dictionary Value**:
   - **Get**: `Value for` → key `clips`. Input: the output of step 3.
   - This gives you the **list** of 5 objects.
5. **Repeat with Each** over the list from step 4:
   - Inside: **Get Dictionary Value** → key `text` of `Repeat Item`.
   - **Add to variable** → variable `Texts` (accumulates the 5 texts).
   *(Simpler alternative: use "Get Dictionary Value" with key `text` directly on
   the list; Shortcuts returns all the `text` values.)*
6. **Choose from List**:
   - List: variable `Texts`.
   - **Prompt**: `Choose a text`.
7. **Copy to Clipboard**: input = the result of step 6.
8. *(Optional)* **Show Notification**: "Copied ✓" to confirm.

### "Replace selection" variant
If you want it to **replace the selection** instead of copying when launched from
the **share sheet on selected text**:
- In **Shortcut settings** enable **Show in Share Sheet** and accept **Text**
  input.
- As the **last action**, instead of "Copy to Clipboard", finish with the action
  that **returns** the chosen text (leave step 6's result as the shortcut's
  output). When run from the selection, iOS replaces the text.

---

## How to launch it

- **Action Button / Back Tap / widget / home**: "view and copy" mode.
- **Share sheet** on selected text: "replace selection" mode (requires the
  variant above).

## Notes

- The token travels in the header, always over **HTTPS**.
- If you get `401`, the token was revoked: generate a new one at `/devices`.
- Here you see **only what was uploaded from this iPhone**. For the pool of all
  devices (Windows included), use [View Shared](ver-compartido.md).
