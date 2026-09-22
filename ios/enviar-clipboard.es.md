[English](enviar-clipboard.md) · **Español**

# Atajo iOS — "Enviar Clipboard" (subir al servidor)

Atajo para **compartir un texto** desde el iPhone al portapapeles compartido.
Funciona de dos formas con un único atajo:

- **Desde el menú de compartir** sobre texto seleccionado → sube la selección.
- **Suelto** (botón Acción, Back Tap, widget, home) → sube lo que tengas en el
  **portapapeles** del iPhone.

---

## Requisitos

- **URL del servidor** — ej. `https://tu-app.vercel.app`
- **Token del dispositivo** — generado en el panel web (`/devices`). Secreto.

> Consume el endpoint `POST /api/clip` (contrato abajo), que ya está
> implementado en el backend.

---

## Contrato del endpoint

**Request**
```
POST /api/clip
Authorization: Bearer <DEVICE_TOKEN>
Content-Type: application/json

{ "text": "el texto a compartir" }
```

**Response `201`**
```json
{ "id": 43, "created_at": "2026-09-21T18:10:00Z" }
```
- `400` si falta `text`; `401` si el token es inválido/revocado; `413` si el
  texto supera 100 000 caracteres.

---

## Cómo construir el atajo (app Atajos)

1. **Atajos** → **+** (nuevo). Nómbralo `Enviar Clipboard`.
2. En **Ajustes del atajo** (icono ⓘ o el interruptor de ajustes):
   - Activa **Mostrar en pantalla de compartir**.
   - En **Tipos de entrada aceptados** deja solo **Texto** (quita el resto para
     que aparezca únicamente al seleccionar texto).
3. **Obtener variable** → **Entrada del atajo** (Shortcut Input). Esta es la
   selección cuando lo lanzas desde el menú de compartir.
4. **Si** (If): *Entrada del atajo* → **tiene algún valor**.
   - **Si (tiene valor):** añade **Definir variable** `Contenido` = *Entrada del
     atajo*.
   - **Si no:** añade **Obtener del portapapeles** (Get Clipboard) y luego
     **Definir variable** `Contenido` = *Portapapeles*.
   - Cierra el **Si**.
5. **Obtener contenido de la URL** (Get Contents of URL):
   - **URL**: `https://tu-app.vercel.app/api/clip`
   - **Mostrar más** → **Método**: `POST`.
   - **Encabezados**:
     - `Authorization` = `Bearer TU_TOKEN`
     - `Content-Type` = `application/json`
   - **Cuerpo de la petición**: `JSON`
     - Añade un campo **texto** con clave `text` y valor = variable `Contenido`.
6. *(Opcional)* **Mostrar notificación**: "Compartido ✓".

> Versión mínima (sin el paso "Si"): si solo quieres subir **la selección**,
> usa directamente *Entrada del atajo* como `text`. Si solo quieres subir el
> **portapapeles**, empieza con *Obtener del portapapeles*. El paso 4 combina
> ambos comportamientos en un solo atajo.

---

## Cómo lanzarlo

- **Menú de compartir** sobre texto seleccionado → sube esa selección.
- **Botón Acción / Back Tap / widget / home** → sube el portapapeles actual.

## Notas

- Empareja con **"Ver Personal"** ([ver-clipboard.es.md](ver-clipboard.es.md))
  para recuperar lo que subiste (o lo que subió tu PC).
- Si recibes `401`, el token fue revocado: genera otro en `/devices`.
- El token viaja en el header, siempre sobre **HTTPS**.
