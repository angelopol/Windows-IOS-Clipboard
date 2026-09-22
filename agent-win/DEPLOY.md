**English** · [Español](DEPLOY.es.md)

# Build, release and auto-start the Windows agent

## 1. Build the `.exe`

### Option A — GitHub Actions (recommended)
The workflow [`.github/workflows/release.yml`](../.github/workflows/release.yml)
builds `ClipboardAgent.exe` on a Windows runner and attaches it to a GitHub
Release when you push a tag. No local Python needed.

```bash
git tag v0.1.0
git push origin v0.1.0
```

The release appears at `https://github.com/<user>/<repo>/releases` with the
`.exe` attached. You can also run the workflow manually (workflow_dispatch) to
get the `.exe` as a build artifact without creating a release.

### Option B — Local build
Requires Python 3.10+ on PATH:

```powershell
cd agent-win
.\build.ps1
# -> dist\ClipboardAgent.exe
```

## 2. Configure

Before auto-start, the agent needs the server URL and a device token
(from `/devices`). Either:

- Run `ClipboardAgent.exe` once — a dialog asks for both and saves them to
  `%APPDATA%\ClipboardAgent\config.json`; or
- Set env vars `CLIPBOARD_SERVER_URL` and `CLIPBOARD_TOKEN` (take priority).

## 3. Auto-start

### Recommended — Task Scheduler at logon
Runs in your user session, so clipboard, the `Ctrl+Alt+V` hotkey and the tray
icon all work.

```powershell
cd agent-win\service
.\install-autostart.ps1            # uses ..\dist\ClipboardAgent.exe
# uninstall:  .\uninstall-autostart.ps1
```

### On request — NSSM service (with a big caveat)

> ⚠️ A Windows service runs in **session 0**, isolated from your desktop. On
> Windows 10/11 the agent then **cannot** read your clipboard, receive the
> global hotkey, or show a tray icon. The service starts, but its interactive
> features do nothing. Prefer Task Scheduler above. This is provided because
> NSSM was explicitly requested.

```powershell
# needs NSSM on PATH (choco install nssm)
cd agent-win\service
.\nssm-install.ps1 -ServerUrl "https://your-app.vercel.app" -Token "clip_XXXX"
# uninstall:  .\nssm-uninstall.ps1
```

The NSSM script passes config via environment variables (a service can't show
the setup dialog) and writes logs to `%APPDATA%\ClipboardAgent\service.log`.

## Why a service doesn't fit this agent

The agent is an interactive desktop app: it watches the **user-session**
clipboard, captures a **global hotkey**, shows a **tray icon** and a picker
window. All of these require the logged-in user's session, which a session-0
service does not have. "Always on, starts by itself" is best achieved with
Task Scheduler at logon — that's the intended deployment.
