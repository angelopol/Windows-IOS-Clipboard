[English](ver-clipboard.md) · **Español**

# Atajo iOS — "Ver Personal" (últimos 5 de este iPhone)

Atajo para **ver tu clipboard personal**: pide al servidor los últimos 5 textos
que subiste **desde este iPhone**, los muestra en un menú y copia el que elijas.
Para ver lo de todos tus dispositivos, usa [Ver Compartido](ver-compartido.es.md).

---

## Requisitos

- **URL del servidor** — ej. `https://tu-app.vercel.app`
- **Token del dispositivo** — se genera en el panel web (`/devices`) y se pega
  en el atajo. Es secreto: no lo compartas.

> El atajo consume `GET /api/clips?scope=personal`. El `scope=personal` filtra
> por el dispositivo del token, así que solo verás lo subido desde este iPhone.

---

## Contrato del endpoint que usa este atajo

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
    { "id": 42, "text": "primer texto (el más reciente)", "created_at": "2026-09-21T18:00:00Z" },
    { "id": 41, "text": "segundo texto", "created_at": "2026-09-21T17:55:00Z" },
    { "id": 40, "text": "tercero", "created_at": "2026-09-21T17:40:00Z" },
    { "id": 39, "text": "cuarto", "created_at": "2026-09-21T17:10:00Z" },
    { "id": 38, "text": "quinto", "created_at": "2026-09-21T16:30:00Z" }
  ]
}
```
- Orden: **más reciente primero**. Máximo 5.
- La respuesta incluye `"scope": "personal"`.
- Errores: `401` si el token es inválido/revocado.

---

## Cómo construir el atajo (app Atajos)

1. Abre **Atajos** → **+** (nuevo atajo). Nómbralo `Ver Personal`.
2. **Texto** → escribe la URL base del servidor, ej. `https://tu-app.vercel.app`.
   (Guardarla en una acción de Texto facilita cambiarla luego.)
3. **Obtener contenido de la URL** (Get Contents of URL):
   - **URL**: la variable del paso 2 + `/api/clips?scope=personal` → queda
     `https://tu-app.vercel.app/api/clips?scope=personal`.
   - Toca **Mostrar más** → **Método**: `GET`.
   - **Encabezados** (Headers) → añade:
     - `Authorization` = `Bearer TU_TOKEN`  *(pega tu token aquí)*
     - `Accept` = `application/json`
4. **Obtener valor del diccionario** (Get Dictionary Value):
   - **Obtener**: `Valor por` → clave `clips`. Entrada: la salida del paso 3.
   - Esto te da la **lista** de 5 objetos.
5. **Obtener valor del diccionario** otra vez, sobre la lista del paso 4:
   - **Obtener**: `Valor por` → clave `text`.
   - **Entrada: la lista del paso 4** (no un elemento suelto — no uses aquí un
     bucle "Repetir con cada"). Atajos aplica la clave a todos los elementos de
     la lista de una vez y devuelve una lista plana de exactamente 5 textos.
     Esto es lo importante: saltarse este paso y elegir directo de la lista del
     paso 4 es la causa #1 de ver "el registro completo" en vez de texto limpio
     — ver Solución de problemas más abajo.
6. **Elegir de la lista** (Choose from List):
   - Lista: la **salida del paso 5** (la lista plana de textos — no la lista
     del paso 4 con objetos `{id, text, created_at}`).
   - **Solicitar**: `Elige un texto`.
7. **Copiar al portapapeles** (Copy to Clipboard): entrada = resultado del
   paso 6.
8. *(Opcional)* **Mostrar notificación**: "Copiado ✓" para confirmar.

### Solución de problemas: veo más de 5 elementos / datos crudos en vez de texto limpio
Esto siempre significa que **Elegir de la lista** (paso 6) apunta a la variable
equivocada. Pasa de dos formas:
- **Elegir directo de la lista del paso 4**, saltándote el paso 5. Cada fila
  muestra entonces el registro completo `{id, text, created_at}` — se ve como
  "toda la lista" en vez de 5 líneas limpias.
- **Usar "Repetir con cada" + "Añadir a variable"** para armar la lista a mano.
  Si esa variable no se reinicia en un solo lugar, o el atajo se vuelve a
  ejecutar antes de terminar, las entradas se pueden ir acumulando entre
  ejecuciones. Quitar el bucle (paso 5 de arriba) elimina este problema por
  completo — no hay nada que acumular.

Si tu atajo ya tiene la versión con Repetir/Añadir a variable, borra esas dos
acciones y sustitúyelas por la única acción "Obtener valor del diccionario
(clave: text) sobre la lista del paso 4" del paso 5.

### Solución de problemas: el atajo me pide elegir un valor dos veces / me muestra el JSON crudo para tocarlo a mano
Esto significa que el campo **Clave** de una acción "Obtener valor del
diccionario" se quedó vacío en vez de tener la clave escrita. Atajos tiene dos
modos para ese campo:
- **Clave escrita** (correcto): toca el campo **Clave** y escribe el texto
  literal `clips` (paso 4) o `text` (paso 5). Esto extrae el valor
  automáticamente, siempre, sin preguntar nada.
- **Clave vacía / "Preguntar cada vez"** (incorrecto): Atajos en su lugar te
  muestra el JSON crudo en cada ejecución y te obliga a tocarlo a mano —
  primero para elegir qué elemento, luego para elegir qué campo dentro de ese
  elemento. Eso es exactamente la doble pregunta que estás viendo.

Arreglo: abre el atajo, toca cada acción "Obtener valor del diccionario", y
asegúrate de que el campo **Clave** tenga escrito literalmente `clips` o `text`
(no "Preguntar cada vez" ni vacío) — igual para la clave del paso 5. Con ambas
escritas, el atajo extrae los 5 textos en silencio, sin preguntas.

### Variante "reemplazar selección"
Si quieres que, al lanzarlo desde el **menú de compartir sobre texto
seleccionado**, reemplace la selección en vez de copiar:
- En **Ajustes del atajo** activa **Mostrar en pantalla de compartir** y acepta
  entrada de tipo **Texto**.
- Como **última acción**, en lugar de "Copiar al portapapeles", termina con la
  acción que **devuelve** el texto elegido (deja el resultado del paso 6 como
  salida del atajo). Al ejecutarlo desde la selección, iOS reemplaza el texto.

---

## Cómo lanzarlo

- **Botón Acción / Back Tap / widget / home**: modo "ver y copiar".
- **Menú de compartir** sobre texto seleccionado: modo "reemplazar selección"
  (requiere la variante de arriba).

## Notas

- El token viaja en el header, siempre sobre **HTTPS**.
- Si recibes `401`, el token fue revocado: genera uno nuevo en `/devices`.
- Aquí ves **solo lo subido desde este iPhone**. Para el pool de todos los
  dispositivos (Windows incluido), usa [Ver Compartido](ver-compartido.es.md).
