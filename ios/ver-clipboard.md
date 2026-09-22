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
5. **Get Dictionary Value** again, on the list from step 4:
   - **Get**: `Value for` → key `text`.
   - **Input: the list from step 4** (not a single item — don't use a "Repeat
     with Each" loop here). Shortcuts applies the key to every item in the list
     at once and returns a flat list of exactly 5 text strings. This is the
     important part: skipping this and choosing straight from the step-4 list
     is the #1 cause of seeing "the whole record" instead of clean text — see
     Troubleshooting below.
6. **Choose from List**:
   - List: the **output of step 5** (the flat list of texts — not step 4's
     list of `{id, text, created_at}` objects).
   - **Prompt**: `Choose a text`.
7. **Copy to Clipboard**: input = the result of step 6.
8. *(Optional)* **Show Notification**: "Copied ✓" to confirm.

### Troubleshooting: I get more than 5 items / raw data instead of clean text
This always means **Choose from List** (step 6) is pointed at the wrong
variable. Two ways this happens:
- **Choosing from step 4's list directly**, skipping step 5. Each row then
  shows the full `{id, text, created_at}` record — it looks like "the whole
  list" instead of 5 clean lines.
- **Using "Repeat with Each" + "Add to Variable"** to build the list by hand.
  If that variable isn't reset in one place, or the shortcut is re-run before
  finishing, entries can pile up across runs. Removing the loop (step 5 above)
  removes this failure mode entirely — there's nothing to accumulate.

If your shortcut already has the Repeat/Add-to-Variable version, delete those
two actions and replace them with the single "Get Dictionary Value (key: text)
on the step-4 list" action from step 5.

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
