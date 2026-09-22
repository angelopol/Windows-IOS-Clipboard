[English](SECURITY.md) · **Español**

# Seguridad y privacidad

## Modelo actual

- **Transporte**: todo va sobre HTTPS (Vercel lo fuerza).
- **Autenticación de dispositivos**: token por dispositivo (`Bearer`). En la
  base solo se guarda su **hash SHA-256**; el token en claro se muestra una vez.
  Se puede **revocar** individualmente desde `/devices`.
- **Cuentas**: contraseña con **bcrypt**; sesión web en cookie **httpOnly**
  (JWT firmado con `AUTH_SECRET`).
- **Aislamiento**: cada consulta filtra por `user_id`; un token solo ve los
  clips de su cuenta.
- **Límites**: textos de hasta 100 000 caracteres; se retienen 20 por cuenta
  (el resto se poda). Dedup de duplicados consecutivos.

El servidor **puede leer** el texto en claro. Es lo que se decidió para el MVP
(HTTPS + auth), a cambio de simplicidad y de que los atajos de iOS funcionen.

## Por qué NO hay cifrado extremo-a-extremo (todavía)

E2E significaría que el servidor solo guardara **texto cifrado** y que la clave
viviera solo en los dispositivos. El bloqueo está en **iOS Shortcuts**:

- La app Atajos **no tiene acciones de cifrado** (no hay AES con passphrase, ni
  HMAC, ni derivación de clave). No se puede cifrar/descifrar el texto en el
  iPhone dentro de un atajo.
- Si ciframos en Windows, el iPhone **no podría leer** lo compartido, y
  viceversa. E2E rompería el lado iOS.

Conclusión: E2E real exige **sustituir los atajos por una app iOS nativa** (o una
app con extensión de teclado/compartir) que haga la criptografía. Es un cambio
de alcance grande y queda fuera de este MVP.

### Camino futuro para E2E (si algún día se hace app nativa)

1. Passphrase por cuenta → derivar clave con **PBKDF2/scrypt/Argon2**.
2. Cifrar cada texto con **AES-256-GCM** (nonce aleatorio por mensaje) en el
   cliente; enviar `{ciphertext, nonce, tag}` en base64.
3. El servidor guarda el blob tal cual (columna `text` = ciphertext).
4. Descifrar en el cliente al leer. La clave **nunca** sale del dispositivo.
5. Migración: columna nueva o flag `encrypted` para convivir con clips en claro.

## Endurecimiento recomendado (siguiente iteración, sin tocar iOS)

- **Rate limiting** por token/IP en `/api/clip` y `/api/auth/*` (p. ej. con
  Upstash Redis) para frenar abuso y fuerza bruta.
- **Bloqueo de login** tras N intentos fallidos.
- **Rotación/caducidad** opcional de tokens de dispositivo.
- **Cabeceras de seguridad** (CSP, HSTS) en las respuestas del panel.
- **Auditoría**: registrar `last_seen` ya existe; añadir IP/última acción.
- **Purga por tiempo**: borrar clips más antiguos de X días además del tope de 20.
