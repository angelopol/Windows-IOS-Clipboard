# Clipboard multiplataforma (Windows ↔ iPhone)

Portapapeles compartido entre dispositivos a través de un servidor central.
Copia algo en un dispositivo y pégalo en otro: los últimos textos de tu cuenta
están disponibles en Windows y en el iPhone.

## Cómo funciona

- **Servidor (Vercel)** guarda una lista corta (últimos ~20, se muestran 5) por
  cuenta. No hay sincronización en tiempo real: **push al copiar + pull bajo
  demanda**, que encaja con serverless.
- **Windows**: un agente en background envía al servidor cada vez que copias, y
  con `Ctrl+Alt+V` muestra los textos compartidos para pegar. *(Fase 3)*
- **iPhone**: atajos de la app Atajos para enviar (clipboard/selección) y ver
  los últimos 5. *(Fase 2)*
- **Web**: login y panel para enlazar/revocar dispositivos mediante tokens.

```
  iPhone (Shortcuts)          Windows (agente + tray)
        │  ▲                          │  ▲
   POST │  │ GET                 POST │  │ GET (Ctrl+Alt+V)
        ▼  │                          ▼  │
   ┌─────────────────────────────────────────┐
   │   Vercel — Next.js  (/web)               │
   │   /api/clip  /api/clips  /api/devices    │
   └───────────────────┬─────────────────────┘
                        ▼
                 Vercel Postgres
```

## Estructura del repo

| Carpeta | Qué es | Estado |
|---------|--------|--------|
| [`web/`](web/) | API + panel de auth (Next.js + Neon, Vercel) | ✅ Fase 0 + 1 |
| [`ios/`](ios/) | Atajos de Shortcuts + guías | ✅ Fase 2 |
| [`agent-win/`](agent-win/) | Agente Windows (Python + tray) | ✅ Fase 3 |

## Roadmap

- [x] **Fase 0** — API `/api/clip` + `/api/clips`, DB (Neon), auth por token
- [x] **Fase 1** — Web: login/registro + panel de dispositivos
- [x] **Fase 2** — Atajos iPhone: [enviar](ios/enviar-clipboard.md) + [ver](ios/ver-clipboard.md)
- [x] **Fase 3** — Agente Windows (auto-push al copiar + `Ctrl+Alt+V` para pegar)
- [x] **Fase 4** — Pulido: dedup (server) + reintentos offline (agente). E2E
  documentado como no viable con Shortcuts → ver [`SECURITY.md`](SECURITY.md)

## Empezar

Ver [`web/README.md`](web/README.md) para levantar el servidor y desplegar en
Vercel. Después, en `/devices`, genera un token por dispositivo y úsalo en el
atajo de iOS ([`ios/ver-clipboard.md`](ios/ver-clipboard.md)) o en el agente de
Windows.

## Seguridad

- Tokens de dispositivo revocables, guardados como hash SHA-256.
- Contraseñas con bcrypt, sesión en cookie httpOnly (JWT).
- Todo sobre HTTPS. El cifrado E2E no es viable con Shortcuts de iOS; detalle y
  camino futuro en [`SECURITY.md`](SECURITY.md).
