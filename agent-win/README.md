**English** · [Español](README.es.md)

# Clipboard — Windows Agent

Background agent that connects your PC to the shared clipboard:

- **Auto-uploads** to the server every time you copy something (`Ctrl+C`).
- **`Ctrl+Alt+V`** opens a menu with the last 5 shared texts and pastes the one
  you pick into the app you were in.
- Lives in the **system tray** with a menu to pause auto-upload or quit.

## Requirements

- Windows 10/11 and Python 3.10+.
- A device token generated in the web panel (`/devices`).

## Install (development mode)

```powershell
cd agent-win
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
python agent.py
```

The first time it will ask for the **server URL** and the **token**. They are
saved in `%APPDATA%\ClipboardAgent\config.json`.

> Dialog-free alternative: set the environment variables
> `CLIPBOARD_SERVER_URL` and `CLIPBOARD_TOKEN` (they take priority over the file).

## Usage

| Action | How |
|--------|-----|
| Share a text | Normal copy (`Ctrl+C`) → uploads by itself |
| Paste from shared | `Ctrl+Alt+V` → pick with arrows or keys `1`–`5` → Enter |
| Pause auto-upload | Click the tray icon → *Auto-upload: ON/OFF* |
| Quit | Tray menu → *Quit* |

## Package as .exe (PyInstaller)

```powershell
pip install pyinstaller
pyinstaller --noconsole --onefile --name ClipboardAgent agent.py
```

The executable lands in `dist\ClipboardAgent.exe`.

### Auto-start with Windows

Create a shortcut to `ClipboardAgent.exe` (or to `pythonw agent.py`) in:

```
%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup
```

## Notes and limitations

- Copy detection is by **polling** every ~1 s (simple and stable). Very rapid
  copies may merge; fine for normal use.
- The watcher ignores **non-textual** content (images) and avoids the "echo" of
  text you just pasted from the shared pool.
- **Offline**: uploads that fail due to network are stored in an in-memory queue
  and retried every ~15 s (up to 50 pending). The server drops duplicates, so
  retrying is safe. The queue does not persist across agent restarts.
- The `keyboard` library captures the global hotkey; on some machines with
  strict policies it may require running as administrator.
- When pasting, the agent restores focus to the previous window via the Windows
  API. If some app doesn't accept the automatic `Ctrl+V`, the text is already in
  the clipboard: paste it manually.
- Plaintext only, over HTTPS + token. E2E encryption: see
  [`../SECURITY.md`](../SECURITY.md).
