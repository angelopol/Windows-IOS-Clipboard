**English** · [Español](SECURITY.es.md)

# Security and privacy

## Current model

- **Transport**: everything goes over HTTPS (Vercel enforces it).
- **Device authentication**: per-device token (`Bearer`). The database stores
  only its **SHA-256 hash**; the plaintext token is shown once. It can be
  **revoked** individually from `/devices`.
- **Accounts**: password with **bcrypt**; web session in an **httpOnly** cookie
  (JWT signed with `AUTH_SECRET`).
- **Isolation**: every query filters by `user_id`; a token only sees its own
  account's clips.
- **Limits**: texts up to 100,000 characters; 20 retained per account (the rest
  are pruned). Dedup of consecutive duplicates.

The server **can read** the plaintext. That's what was decided for the MVP
(HTTPS + auth), in exchange for simplicity and for the iOS shortcuts to work.

## Why there is NO end-to-end encryption (yet)

E2E would mean the server only stores **ciphertext** and the key lives only on
the devices. The blocker is **iOS Shortcuts**:

- The Shortcuts app **has no crypto actions** (no AES with passphrase, no HMAC,
  no key derivation). You cannot encrypt/decrypt the text on the iPhone inside a
  shortcut.
- If we encrypt on Windows, the iPhone **couldn't read** the shared pool, and
  vice versa. E2E would break the iOS side.

Conclusion: real E2E requires **replacing the shortcuts with a native iOS app**
(or an app with a keyboard/share extension) that does the crypto. That's a large
scope change and is out of this MVP.

### Future path for E2E (if a native app is ever built)

1. Per-account passphrase → derive a key with **PBKDF2/scrypt/Argon2**.
2. Encrypt each text with **AES-256-GCM** (random nonce per message) on the
   client; send `{ciphertext, nonce, tag}` as base64.
3. The server stores the blob as-is (`text` column = ciphertext).
4. Decrypt on the client when reading. The key **never** leaves the device.
5. Migration: a new column or an `encrypted` flag to coexist with plaintext clips.

## Recommended hardening (next iteration, without touching iOS)

- **Rate limiting** per token/IP on `/api/clip` and `/api/auth/*` (e.g. with
  Upstash Redis) to curb abuse and brute force.
- **Login lockout** after N failed attempts.
- Optional **rotation/expiry** of device tokens.
- **Security headers** (CSP, HSTS) on the panel responses.
- **Audit**: `last_seen` logging already exists; add IP/last action.
- **Time-based purge**: delete clips older than X days on top of the 20 cap.
