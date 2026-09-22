**English** · [Español](enviar-clipboard.es.md)

# iOS Shortcut — "Send Clipboard" (upload to the server)

Shortcut to **share a text** from the iPhone to the shared clipboard. It works
two ways with a single shortcut:

- **From the share sheet** on selected text → uploads the selection.
- **Standalone** (Action Button, Back Tap, widget, home) → uploads whatever is
  in the iPhone **clipboard**.

---

## Requirements

- **Server URL** — e.g. `https://your-app.vercel.app`
- **Device token** — generated in the web panel (`/devices`). Secret.

> Uses the `POST /api/clip` endpoint (contract below), already implemented in the
> backend.

---

## Endpoint contract

**Request**
```
POST /api/clip
Authorization: Bearer <DEVICE_TOKEN>
Content-Type: application/json

{ "text": "the text to share" }
```

**Response `201`**
```json
{ "id": 43, "created_at": "2026-09-21T18:10:00Z" }
```
- `400` if `text` is missing; `401` if the token is invalid/revoked; `413` if the
  text exceeds 100,000 characters.

---

## How to build the shortcut (Shortcuts app)

1. **Shortcuts** → **+** (new). Name it `Send Clipboard`.
2. In **Shortcut settings** (the ⓘ icon or the settings toggle):
   - Enable **Show in Share Sheet**.
   - Under **Accepted input types** keep only **Text** (remove the rest so it
     only appears when text is selected).
3. **Get variable** → **Shortcut Input**. This is the selection when launched
   from the share sheet.
4. **If**: *Shortcut Input* → **has any value**.
   - **If (has value):** add **Set variable** `Content` = *Shortcut Input*.
   - **Otherwise:** add **Get Clipboard** and then **Set variable** `Content` =
     *Clipboard*.
   - Close the **If**.
5. **Get Contents of URL**:
   - **URL**: `https://your-app.vercel.app/api/clip`
   - **Show more** → **Method**: `POST`.
   - **Headers**:
     - `Authorization` = `Bearer YOUR_TOKEN`
     - `Content-Type` = `application/json`
   - **Request Body**: `JSON`
     - Add a **text** field with key `text` and value = the `Content` variable.
6. *(Optional)* **Show Notification**: "Shared ✓".

> Minimal version (without the "If" step): if you only want to upload **the
> selection**, use *Shortcut Input* directly as `text`. If you only want to
> upload the **clipboard**, start with *Get Clipboard*. Step 4 combines both
> behaviors in a single shortcut.

---

## How to launch it

- **Share sheet** on selected text → uploads that selection.
- **Action Button / Back Tap / widget / home** → uploads the current clipboard.

## Notes

- Pairs with **"View Personal"** ([ver-clipboard.md](ver-clipboard.md)) to
  retrieve what you uploaded (or what your PC uploaded).
- If you get `401`, the token was revoked: generate another one at `/devices`.
- The token travels in the header, always over **HTTPS**.
