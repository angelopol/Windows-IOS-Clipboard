**English** · [Español](README.es.md)

# Clipboard — Web / API (Vercel)

Backend and auth panel for the cross-platform clipboard. Next.js (App Router)
+ Neon Postgres.

## Local setup

1. **Provision a Neon database** on Vercel (Storage → Create → Neon) or create a
   free one at [neon.tech](https://neon.tech) and copy its connection string.
2. **Environment variables** — copy the example and fill it in:
   ```bash
   cp .env.example .env.local
   ```
   - `DATABASE_URL` — Neon connection string. On Vercel: `vercel env pull .env.local`.
   - `AUTH_SECRET` — generate one:
     ```bash
     node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"
     ```
3. **Create the tables**:
   ```bash
   npm run db:setup
   ```
4. **Run it**:
   ```bash
   npm run dev
   ```
   Open http://localhost:3000 → create an account → `/devices` → generate a token.

## Deploy to Vercel

1. `vercel` (or connect the repo in the dashboard). Root directory: `web`.
2. In the Vercel project add the **Neon** integration and the env vars
   `DATABASE_URL` (provided by the integration) and `AUTH_SECRET`.
3. Run the schema once against the production database:
   ```bash
   vercel env pull .env.local   # pulls the production DATABASE_URL
   npm run db:setup
   ```

## API

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `POST` | `/api/clip` | `Bearer <device-token>` | Upload a text `{ "text": "..." }` |
| `GET` | `/api/clips?scope=personal\|shared` | `Bearer <device-token>` | Last 5. `personal` = this device only; `shared` (default) = all |
| `POST` | `/api/devices` | session (cookie) | Create device → returns the token once |
| `GET` | `/api/devices` | session | List devices |
| `DELETE` | `/api/devices/:id` | session | Revoke a device |
| `POST` | `/api/auth/register` | — | `{ email, password }` |
| `POST` | `/api/auth/login` | — | `{ email, password }` |
| `POST` | `/api/auth/logout` | — | Log out |

### Test with curl

```bash
# Upload a text
curl -X POST https://YOUR-APP.vercel.app/api/clip \
  -H "Authorization: Bearer clip_XXXX" \
  -H "Content-Type: application/json" \
  -d '{"text":"hello from curl"}'

# Read the last 5
curl https://YOUR-APP.vercel.app/api/clips \
  -H "Authorization: Bearer clip_XXXX"
```

## Data model

See [`schema.sql`](schema.sql): `users`, `devices` (token stored as a SHA-256
hash), `clips` (pruned to the 20 most recent per user on each insert).

## Notes / pending

- DB via the **Neon** SDK (`@neondatabase/serverless`). The layer lives in
  [`lib/db.ts`](lib/db.ts); its `sql` returns the row array directly. Env var:
  `DATABASE_URL` (also accepts `POSTGRES_URL`).
- Auth is email + password (JWT session in an httpOnly cookie). Magic link is a
  future improvement.
- No E2E encryption yet: the server sees plaintext (protected by HTTPS + token).
  E2E is optional Phase 4.
