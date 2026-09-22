# Clipboard — Agente de Windows

Agente en segundo plano que conecta tu PC con el portapapeles compartido:

- **Auto-envía** al servidor cada vez que copias algo (`Ctrl+C`).
- **`Ctrl+Alt+V`** abre un menú con los últimos 5 textos compartidos y pega el
  que elijas en la app donde estabas.
- Vive en la **bandeja del sistema** (tray) con menú para pausar el auto-envío
  o salir.

## Requisitos

- Windows 10/11 y Python 3.10+.
- Un token de dispositivo generado en el panel web (`/devices`).

## Instalación (modo desarrollo)

```powershell
cd agent-win
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
python agent.py
```

La primera vez te pedirá la **URL del servidor** y el **token**. Se guardan en
`%APPDATA%\ClipboardAgent\config.json`.

> Alternativa sin diálogo: define las variables de entorno
> `CLIPBOARD_SERVER_URL` y `CLIPBOARD_TOKEN` (tienen prioridad sobre el archivo).

## Uso

| Acción | Cómo |
|--------|------|
| Compartir un texto | Copia normal (`Ctrl+C`) → se sube solo |
| Pegar de lo compartido | `Ctrl+Alt+V` → elige con flechas o teclas `1`–`5` → Enter |
| Pausar auto-envío | Click en el icono de la bandeja → *Auto-enviar: ON/OFF* |
| Salir | Menú de la bandeja → *Salir* |

## Empaquetar como .exe (PyInstaller)

```powershell
pip install pyinstaller
pyinstaller --noconsole --onefile --name ClipboardAgent agent.py
```

El ejecutable queda en `dist\ClipboardAgent.exe`.

### Arranque automático con Windows

Crea un acceso directo a `ClipboardAgent.exe` (o a `pythonw agent.py`) en:

```
%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup
```

## Notas y limitaciones

- La detección de copia es por **sondeo** cada ~1 s (simple y estable). Copias
  muy seguidas pueden fusionarse; suficiente para el uso normal.
- El watcher ignora contenido **no textual** (imágenes) y evita el "eco" del
  texto que acaba de pegar desde lo compartido.
- **Sin conexión**: los envíos que fallan por red se guardan en una cola en
  memoria y se reintentan cada ~15 s (hasta 50 pendientes). Los duplicados los
  descarta el servidor, así que reintentar es seguro. La cola no persiste entre
  reinicios del agente.
- La librería `keyboard` captura el hotkey global; en algunos equipos con
  políticas estrictas puede requerir ejecutar como administrador.
- Al pegar, el agente devuelve el foco a la ventana anterior con la API de
  Windows. Si alguna app no acepta el `Ctrl+V` automático, el texto ya quedó en
  el portapapeles: pégalo manualmente.
- Solo texto plano, y sobre HTTPS + token. Cifrado E2E: Fase 4 (pendiente).
