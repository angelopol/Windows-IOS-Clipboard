# Atajo iOS — "Ver Compartido" (últimos 5 de todos los dispositivos)

Atajo para **ver el clipboard compartido**: los últimos 5 textos subidos desde
**cualquier** dispositivo de tu cuenta (Windows, iPhone, otros). Los muestra en
un menú y copia el que elijas. Para ver solo lo tuyo de este iPhone, usa
[Ver Personal](ver-clipboard.md).

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
4. **Obtener valor del diccionario** → clave `clips` (te da la lista).
5. **Repetir con cada** sobre la lista → dentro, **Obtener valor del
   diccionario** clave `text` → **Añadir a variable** `Textos`.
   *(Alternativa: "Obtener valor del diccionario" con clave `text` sobre la
   lista devuelve todos los `text` directamente.)*
6. **Elegir de la lista** → lista = `Textos`, solicitar `Elige un texto`.
7. **Copiar al portapapeles** → resultado del paso 6.
8. *(Opcional)* **Mostrar notificación**: "Copiado ✓".

### Variante "reemplazar selección"
Igual que en [Ver Personal](ver-clipboard.md): activa **Mostrar en pantalla de
compartir** aceptando **Texto**, y deja el texto elegido como **salida** del
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
