# Clipboard — Atajos de iPhone (iOS Shortcuts)

Dos atajos de la app **Atajos** conectan tu iPhone con el portapapeles
compartido. No hay una app que instalar: se construyen a mano siguiendo las
guías (los `.shortcut` firmados son frágiles entre versiones de iOS, así que la
guía reproducible es lo fiable).

| Atajo | Qué hace | Guía |
|-------|----------|------|
| **Enviar Clipboard** | Sube al servidor la selección (menú de compartir) o el portapapeles | [enviar-clipboard.md](enviar-clipboard.md) |
| **Ver Personal** | Últimos 5 textos subidos **desde este iPhone** (`scope=personal`) | [ver-clipboard.md](ver-clipboard.md) |
| **Ver Compartido** | Últimos 5 de **todos los dispositivos** (`scope=shared`) | [ver-compartido.md](ver-compartido.md) |

## Requisitos comunes

- **URL del servidor** (ej. `https://tu-app.vercel.app`) y un **token de
  dispositivo** creado en el panel web (`/devices`). El mismo token vale para
  ambos atajos.

## Cómo lanzarlos (recomendado)

- **Back Tap** (Ajustes → Accesibilidad → Tocar → Tocar la parte posterior):
  asigna *Enviar* a doble toque y *Ver* a triple toque, por ejemplo.
- **Botón Acción** (iPhone 15 Pro y posteriores) para el que más uses.
- **Menú de compartir** sobre texto seleccionado (ideal para *Enviar* la
  selección y para que *Ver* reemplace la selección).

## Límite importante de iOS

Un atajo **no puede** detectar si estás en un campo de texto ni insertar en apps
arbitrarias. Por eso:

- *Ver Clipboard* **reemplaza la selección** solo cuando se lanza desde el menú
  de compartir con texto seleccionado; en cualquier otro caso **copia** al
  portapapeles y tú pegas.
- *Enviar Clipboard* sube la **selección** si se lanza desde compartir, o el
  **portapapeles** si se lanza suelto.

## Personal vs. compartido

- **Personal** (`GET /api/clips?scope=personal`) = solo lo subido desde **este
  dispositivo** (filtra por el token). Es tu historial propio del iPhone.
- **Compartido** (`GET /api/clips?scope=shared`, o sin parámetro) = el pool de
  **todos** los dispositivos de la cuenta. Es lo mismo que ve el agente de
  Windows con `Ctrl+Alt+V`.

## Contratos de API

- `POST /api/clip` — `{ "text": "..." }` con `Authorization: Bearer <token>`.
- `GET /api/clips?scope=personal|shared` — devuelve `{ "clips": [...], "scope" }`
  (últimos 5). Sin `scope`, por defecto `shared`.

Detalle completo en [`web/README.md`](../web/README.md).
