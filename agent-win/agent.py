"""Agente de Windows para el portapapeles multiplataforma.

- Auto-envía al servidor cada vez que copias algo (watcher del portapapeles).
- Con Ctrl+Alt+V muestra los últimos 5 textos compartidos para pegar.
- Vive en la bandeja del sistema (tray) con menú para pausar/salir.
"""
import ctypes
import threading
import time

import keyboard
import pyperclip
import pystray
import requests
from PIL import Image, ImageDraw
from pystray import MenuItem as Item

from config import load_config, save_config, setup_dialog

user32 = ctypes.windll.user32
HOTKEY = "ctrl+alt+v"
POLL_SECONDS = 1.0
MAX_PREVIEW = 80
RETRY_SECONDS = 15.0  # cada cuánto reintentar los envíos que fallaron sin red
MAX_PENDING = 50  # tope de la cola offline (descarta los más antiguos)


class Agent:
    def __init__(self, config):
        self.server = config["server_url"].rstrip("/")
        self.token = config["token"]
        self.auto_send = True
        self.icon = None  # se asigna tras crear el tray
        self._last_clip = None
        self._picker_open = False
        self._stop = threading.Event()
        self._pending = []  # textos que no se pudieron enviar (sin conexión)
        self._lock = threading.Lock()

    # ---------------------------------------------------------------- http
    def _headers(self):
        return {"Authorization": f"Bearer {self.token}"}

    def _notify(self, msg):
        if self.icon is not None:
            try:
                self.icon.notify(msg, "Clipboard")
            except Exception:
                pass

    def _try_post(self, text):
        """Envía un texto. Devuelve True si NO hay que reintentar.

        - Éxito o error de cliente (401/413/…): True (reintentar no ayuda).
        - Error de red (sin conexión): False → se encola para reintentar.
        """
        try:
            r = requests.post(
                f"{self.server}/api/clip",
                json={"text": text},
                headers=self._headers(),
                timeout=10,
            )
            if r.status_code == 401:
                self._notify("Token inválido o revocado. Reconfigura el agente.")
            return True
        except requests.RequestException:
            return False

    def push(self, text):
        if not self._try_post(text):
            with self._lock:
                self._pending.append(text)
                if len(self._pending) > MAX_PENDING:
                    self._pending.pop(0)  # descarta el más antiguo

    def retry_loop(self):
        """Reintenta en segundo plano los envíos que quedaron sin conexión."""
        while not self._stop.is_set():
            time.sleep(RETRY_SECONDS)
            with self._lock:
                batch = self._pending
                self._pending = []
            for text in batch:
                if not self._try_post(text):
                    with self._lock:
                        self._pending.append(text)

    def fetch(self):
        try:
            r = requests.get(
                f"{self.server}/api/clips",
                headers=self._headers(),
                timeout=10,
            )
            if r.status_code == 200:
                return r.json().get("clips", [])
            if r.status_code == 401:
                self._notify("Token inválido o revocado. Reconfigura el agente.")
        except requests.RequestException:
            self._notify("Sin conexión con el servidor.")
        return None

    # ------------------------------------------------------- auto push loop
    def watch_clipboard(self):
        try:
            self._last_clip = pyperclip.paste()
        except Exception:
            self._last_clip = ""

        while not self._stop.is_set():
            time.sleep(POLL_SECONDS)
            if not self.auto_send:
                continue
            try:
                current = pyperclip.paste()
            except Exception:
                continue  # contenido no textual (imagen, etc.)
            if current and current != self._last_clip:
                self._last_clip = current
                self.push(current)

    # ---------------------------------------------------------- hotkey flow
    def on_hotkey(self):
        if self._picker_open:
            return
        # La ventana enfocada AHORA es el destino donde el usuario quiere pegar.
        target_hwnd = user32.GetForegroundWindow()

        clips = self.fetch()
        if not clips:
            return

        self._picker_open = True
        try:
            chosen = show_picker(clips)
        finally:
            self._picker_open = False
        if chosen is None:
            return

        # Devuelve el foco a la app destino y pega.
        try:
            user32.SetForegroundWindow(target_hwnd)
        except Exception:
            pass
        time.sleep(0.15)
        self._last_clip = chosen  # evita el eco (que el watcher lo reenvíe)
        pyperclip.copy(chosen)
        time.sleep(0.05)
        keyboard.send("ctrl+v")


# ------------------------------------------------------------------ picker
def show_picker(clips):
    """Ventana modal con los 5 textos. Devuelve el texto elegido o None.

    Se crea en el hilo que la invoca (el del hotkey), con su propio Tk root.
    """
    import tkinter as tk

    result = {"value": None}
    root = tk.Tk()
    root.title("Clipboard compartido")
    root.attributes("-topmost", True)
    w, h = 520, 280
    sw, sh = root.winfo_screenwidth(), root.winfo_screenheight()
    root.geometry(f"{w}x{h}+{(sw - w) // 2}+{(sh - h) // 3}")
    root.configure(bg="#16181d")

    tk.Label(
        root,
        text="Elige un texto (Enter para pegar, Esc para cancelar)",
        bg="#16181d",
        fg="#9aa0ab",
        font=("Segoe UI", 9),
    ).pack(padx=12, pady=(10, 4), anchor="w")

    listbox = tk.Listbox(
        root,
        bg="#0e1014",
        fg="#e7e9ee",
        selectbackground="#4f8cff",
        selectforeground="#ffffff",
        borderwidth=0,
        highlightthickness=0,
        activestyle="none",
        font=("Segoe UI", 10),
    )
    for i, c in enumerate(clips):
        preview = " ".join(str(c.get("text", "")).split())[:MAX_PREVIEW]
        listbox.insert("end", f"{i + 1}.  {preview}")
    listbox.pack(fill="both", expand=True, padx=12, pady=(0, 12))
    listbox.select_set(0)
    listbox.focus_set()

    def choose(_event=None):
        sel = listbox.curselection()
        if sel:
            result["value"] = clips[sel[0]].get("text", "")
        root.destroy()

    def cancel(_event=None):
        root.destroy()

    def pick_index(idx):
        if 0 <= idx < len(clips):
            result["value"] = clips[idx].get("text", "")
            root.destroy()

    listbox.bind("<Double-Button-1>", choose)
    root.bind("<Return>", choose)
    root.bind("<Escape>", cancel)
    for n in range(1, min(len(clips), 9) + 1):
        root.bind(str(n), lambda _e, idx=n - 1: pick_index(idx))

    root.after(80, lambda: (root.lift(), root.focus_force()))
    root.mainloop()
    return result["value"]


# -------------------------------------------------------------------- tray
def make_image():
    img = Image.new("RGB", (64, 64), "#16181d")
    d = ImageDraw.Draw(img)
    d.rectangle([18, 16, 46, 52], outline="#4f8cff", width=3)
    d.rectangle([26, 10, 38, 20], fill="#4f8cff")
    return img


def main():
    config = load_config()
    if not config:
        config = setup_dialog()
        if not config:
            return
        save_config(config)

    agent = Agent(config)
    threading.Thread(target=agent.watch_clipboard, daemon=True).start()
    threading.Thread(target=agent.retry_loop, daemon=True).start()
    keyboard.add_hotkey(
        HOTKEY,
        lambda: threading.Thread(target=agent.on_hotkey, daemon=True).start(),
    )

    def toggle_auto(icon, _item):
        agent.auto_send = not agent.auto_send
        icon.update_menu()

    def quit_app(icon, _item):
        agent._stop.set()
        icon.stop()

    menu = pystray.Menu(
        Item(
            lambda _item: f"Auto-enviar: {'ON' if agent.auto_send else 'OFF'}",
            toggle_auto,
        ),
        Item("Salir", quit_app),
    )
    icon = pystray.Icon("clipboard", make_image(), "Clipboard compartido", menu)
    agent.icon = icon
    icon.run()


if __name__ == "__main__":
    main()
