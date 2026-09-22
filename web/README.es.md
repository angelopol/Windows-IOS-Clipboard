[English](README.md) · **Español**

# Clipboard — Web / API (Vercel)

Backend y panel de auth del portapapeles multiplataforma. Next.js (App Router)
+ Neon Postgres.

## Puesta en marcha local

1. **Provisiona una base Neon** en Vercel (Storage → Create → Neon) o crea una
   gratis en [neon.tech](https://neon.tech) y copia su connection string.
2. **Variables de entorno** — copia el ejemplo y rellénalo:
   ```bash
   cp .env.example .env.local
   ```
   - `DATABASE_URL` — connection string de Neon. En Vercel: `vercel env pull .env.local`.
   - `AUTH_SECRET` — genera uno:
     ```bash
     node -e "console.log(require('crypto').randomBytes(32).toString('hex'))"
     ```
3. **Crea las tablas**:
   ```bash
   npm run db:setup
   ```
4. **Arranca**:
   ```bash
   npm run dev
   ```
   Abre http://localhost:3000 → crea cuenta → `/devices` → genera un token.

## Despliegue en Vercel

1. `vercel` (o conecta el repo en el dashboard). Root directory: `web`.
2. En el proyecto de Vercel añade la integración **Neon** y las env vars
   `DATABASE_URL` (la da la integración) y `AUTH_SECRET`.
3. Ejecuta el esquema una vez contra la base de producción:
   ```bash
   vercel env pull .env.local   # trae DATABASE_URL de producción
   npm run db:setup
   ```

## API

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| `POST` | `/api/clip` | `Bearer <device-token>` | Sube un texto `{ "text": "..." }` |
| `GET` | `/api/clips?scope=personal\|shared` | `Bearer <device-token>` | Últimos 5. `personal` = solo este dispositivo; `shared` (defecto) = todos |
| `POST` | `/api/devices` | sesión (cookie) | Crea dispositivo → devuelve token una vez |
| `GET` | `/api/devices` | sesión | Lista dispositivos |
| `DELETE` | `/api/devices/:id` | sesión | Revoca un dispositivo |
| `POST` | `/api/auth/register` | — | `{ email, password }` |
| `POST` | `/api/auth/login` | — | `{ email, password }` |
| `POST` | `/api/auth/logout` | — | Cierra sesión |

### Probar con curl

```bash
# Sube un texto
curl -X POST https://TU-APP.vercel.app/api/clip \
  -H "Authorization: Bearer clip_XXXX" \
  -H "Content-Type: application/json" \
  -d '{"text":"hola desde curl"}'

# Lee los últimos 5
curl https://TU-APP.vercel.app/api/clips \
  -H "Authorization: Bearer clip_XXXX"
```

## Modelo de datos

Ver [`schema.sql`](schema.sql): `users`, `devices` (token guardado como hash
SHA-256), `clips` (podados a los 20 más recientes por usuario en cada inserción).

## Notas / pendientes

- DB con el SDK de **Neon** (`@neondatabase/serverless`). La capa está en
  [`lib/db.ts`](lib/db.ts); el `sql` devuelve el array de filas directamente.
  Variable de entorno: `DATABASE_URL` (también acepta `POSTGRES_URL`).
- Auth es email + contraseña (sesión JWT en cookie httpOnly). Magic link queda
  como mejora.
- Sin cifrado E2E aún: el servidor ve el texto en claro (protegido por HTTPS +
  token). E2E es la Fase 4 opcional.
