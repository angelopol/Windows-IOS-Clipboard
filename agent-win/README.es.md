[English](README.md) · **Español**

# Clipboard — Agente de Windows (Python, legacy)

> 🗄️ **Legacy.** El agente de Windows actual es el nativo en C# / WPF en
> [`../agent-win-cs/`](../agent-win-cs/README.es.md) (flyout estilo Win11,
> portapapeles por eventos, auto-arranque por registro). Esta versión Python se
> conserva como referencia.

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

## Compilar, publicar y auto-arrancar

Guía completa en [`DEPLOY.es.md`](DEPLOY.es.md). En resumen:

- **Compilar el `.exe`**: haz push de un tag → GitHub Actions lo compila y
  publica un Release (sin Python local), o ejecuta `.\build.ps1` en local.
- **Arranque automático (recomendado)**: el `.exe` **se auto-registra** al
  iniciar sesión en su primer arranque (Task Scheduler, sesión de usuario —
  portapapeles, hotkey y tray funcionan). Actívalo/desactívalo desde el tray →
  "Iniciar con Windows". También hay un script manual
  (`service\install-autostart.ps1`).
- **Servicio NSSM**: posible pero **no adecuado** — un servicio en session 0 no
  puede acceder al portapapeles, hotkey ni tray. Detalle y advertencia en
  [`DEPLOY.es.md`](DEPLOY.es.md).

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
- Solo texto plano, y sobre HTTPS + token. Cifrado E2E: ver
  [`../SECURITY.es.md`](../SECURITY.es.md).
