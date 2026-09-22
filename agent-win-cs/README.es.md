[English](README.md) · **Español**

# Clipboard — Agente de Windows (C# / WPF)

Agente nativo de Windows que conecta tu PC con el portapapeles compartido.
Reemplaza al agente Python (legacy) con un flyout estilo Windows 11, captura del
portapapeles por eventos y auto-arranque por registro.

- **Auto-envía** al copiar — por eventos (`AddClipboardFormatListener`), sin
  sondeo.
- **`Ctrl+Alt+V`** abre un flyout estilo Win11 (redondeado, sombra, navegación
  por teclado) con los últimos 5 textos compartidos; elige uno para pegarlo
  donde estabas.
- **Icono de bandeja** con: *Auto-enviar*, *Iniciar con Windows*, *Ver
  portapapeles*, *Salir*.
- **Auto-arranque** vía la clave `Run` del registro — se registra solo en el
  primer arranque.

## Requisitos

- Windows 10/11 (x64).
- Para compilar: .NET 9 SDK. Para ejecutar el `.exe` del release: nada
  (self-contained).

## Ejecutar / compilar en local

```powershell
cd agent-win-cs
dotnet run                     # ejecución de desarrollo
# o un exe self-contained de un solo archivo:
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
# -> publish\ClipboardAgent.exe
```

## Release (GitHub Actions)

Haz push de un tag y el CI compila el `.exe` y publica un Release:

```bash
git tag v0.2.0
git push origin v0.2.0
```

Ver [`.github/workflows/release.yml`](../.github/workflows/release.yml).

## Primer arranque

Al ejecutar el `.exe` por primera vez pide la **URL del servidor** y un **token
de dispositivo** (del panel web `/devices`), y los guarda en
`%APPDATA%\ClipboardAgent\config.json` (mismo formato que el agente Python).
También se registra para iniciar con Windows. Cámbialo cuando quieras desde el
tray → **Iniciar con Windows**.

Alternativa: define las variables `CLIPBOARD_SERVER_URL` y `CLIPBOARD_TOKEN`
(tienen prioridad y omiten el registro de auto-arranque).

## Notas

- Misma API que el resto: `POST /api/clip`, `GET /api/clips?scope=shared`.
- Los envíos sin conexión se encolan y reintentan cada 15 s (el servidor
  deduplica, así que reintentar es seguro).
- El flyout es una tarjeta redondeada translúcida (funciona en Win10/11). El
  fondo Mica real queda como posible mejora futura.
- Texto plano sobre HTTPS + token; ver [`../SECURITY.es.md`](../SECURITY.es.md).
