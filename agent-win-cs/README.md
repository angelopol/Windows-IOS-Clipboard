**English** · [Español](README.es.md)

# Clipboard — Windows Agent (C# / WPF)

Native Windows agent that connects your PC to the shared clipboard. Replaces the
legacy Python agent with a Windows 11-style flyout, event-driven clipboard
capture and registry-based auto-start.

- **Auto-uploads** on copy — event-driven (`AddClipboardFormatListener`), no
  polling.
- **`Ctrl+Alt+V`** opens a Win11-style flyout (rounded, shadow, keyboard nav)
  with the last 5 shared texts; pick one to paste it where you were.
- **Tray icon** with: *Auto-enviar*, *Iniciar con Windows*, *Ver portapapeles*,
  *Salir*.
- **Auto-start** via the registry `Run` key — self-registers on first run.

## Requirements

- Windows 10/11 (x64).
- To build: .NET 9 SDK. To run the released `.exe`: nothing (self-contained).

## Run / build locally

```powershell
cd agent-win-cs
dotnet run                     # dev run
# or a single-file self-contained exe:
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
# -> publish\ClipboardAgent.exe
```

## Release (GitHub Actions)

Push a tag and CI builds the `.exe` and publishes a Release:

```bash
git tag v0.2.0
git push origin v0.2.0
```

See [`.github/workflows/release.yml`](../.github/workflows/release.yml).

## First run

Running the `.exe` the first time asks for the **server URL** and a **device
token** (from the web panel `/devices`), saved to
`%APPDATA%\ClipboardAgent\config.json` (shared format with the Python agent).
It also registers itself to start with Windows. Toggle that anytime from the
tray → **Iniciar con Windows**.

Alternatively set env vars `CLIPBOARD_SERVER_URL` and `CLIPBOARD_TOKEN` (they
take priority and skip auto-start registration).

## Notes

- Same API as the rest of the project: `POST /api/clip`, `GET /api/clips?scope=shared`.
- Offline uploads are queued and retried every 15 s (the server dedups, so
  retrying is safe).
- The flyout is a translucent rounded card (works on Win10/11). True Mica
  backdrop is a possible future enhancement.
- Plaintext over HTTPS + token; see [`../SECURITY.md`](../SECURITY.md).
