[English](ver-compartido.md) · **Español**

# Atajo iOS — "Ver Compartido" (últimos 5 de todos los dispositivos)

Atajo para **ver el clipboard compartido**: los últimos 5 textos subidos desde
**cualquier** dispositivo de tu cuenta (Windows, iPhone, otros). Los muestra en
un menú y copia el que elijas. Para ver solo lo tuyo de este iPhone, usa
[Ver Personal](ver-clipboard.es.md).

Es idéntico a "Ver Personal" salvo por la URL (sin `scope=personal`, o con
`scope=shared`).

---

## Requisitos

- **URL del servidor** — ej. `https://tu-app.vercel.app`
- **Token del dispositivo** — del panel web (`/devices`). El mismo que usan los
  demás atajos.

---

## Contrato del endpoint

**Request**
```
GET /api/clips?scope=shared
Authorization: Bearer <DEVICE_TOKEN>
Accept: application/json
```
> `scope=shared` es el valor por defecto: llamar a `/api/clips` sin parámetro
> devuelve lo mismo.

**Response `200`**
```json
{
  "clips": [
    { "id": 43, "text": "copiado en el PC", "created_at": "2026-09-21T18:05:00Z" },
    { "id": 42, "text": "copiado en el iPhone", "created_at": "2026-09-21T18:00:00Z" }
  ],
  "scope": "shared"
}
```
- Orden: **más reciente primero**. Máximo 5.
- Errores: `401` si el token es inválido/revocado.

---

## Cómo construir el atajo (app Atajos)

1. Abre **Atajos** → **+** (nuevo atajo). Nómbralo `Ver Compartido`.
2. **Texto** → la URL base del servidor, ej. `https://tu-app.vercel.app`.
3. **Obtener contenido de la URL** (Get Contents of URL):
   - **URL**: la variable del paso 2 + `/api/clips?scope=shared` → queda
     `https://tu-app.vercel.app/api/clips?scope=shared`.
   - **Mostrar más** → **Método**: `GET`.
   - **Encabezados**:
     - `Authorization` = `Bearer TU_TOKEN`
     - `Accept` = `application/json`
4. **Obtener valor del diccionario** → campo **Clave** escrito literalmente
   `clips` (te da la lista de 5 objetos). No dejes Clave vacía ni en "Preguntar
   cada vez" — ver Solución de problemas más abajo.
5. **Obtener valor del diccionario** otra vez → campo **Clave** escrito
   literalmente `text`, **entrada: la lista completa del paso 4** (no un
   elemento suelto — no uses aquí "Repetir con cada"). Atajos devuelve una
   lista plana de exactamente 5 textos.
6. **Elegir de la lista** → lista = la **salida del paso 5** (no la lista del
   paso 4 con objetos `{id, text, created_at}`), solicitar `Elige un texto`.

Si ves más de 5 elementos, JSON crudo, o el atajo te pide tocar valores a mano,
ver Solución de problemas en
[ver-clipboard.es.md](ver-clipboard.es.md#solución-de-problemas-veo-más-de-5-elementos--datos-crudos-en-vez-de-texto-limpio)
y
[aquí](ver-clipboard.es.md#solución-de-problemas-el-atajo-me-pide-elegir-un-valor-dos-veces--me-muestra-el-json-crudo-para-tocarlo-a-mano).
7. **Copiar al portapapeles** → resultado del paso 6.
8. *(Opcional)* **Mostrar notificación**: "Copiado ✓".

### Variante "reemplazar selección"
Igual que en [Ver Personal](ver-clipboard.es.md): activa **Mostrar en pantalla
de compartir** aceptando **Texto**, y deja el texto elegido como **salida** del
atajo (sin "Copiar al portapapeles") para reemplazar la selección al lanzarlo
desde el menú de compartir.

---

## Cómo lanzarlo

- **Botón Acción / Back Tap / widget / home**: modo "ver y copiar".
- **Menú de compartir** sobre texto seleccionado: modo "reemplazar selección".

## Notas

- Este es el equivalente iOS del menú `Ctrl+Alt+V` del agente de Windows: ambos
  leen el mismo pool compartido.
- El token viaja en el header, siempre sobre **HTTPS**.
- Si recibes `401`, genera un token nuevo en `/devices`.
