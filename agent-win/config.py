"""Carga y guardado de la configuración del agente (URL del servidor + token)."""
import os
import json
import tkinter as tk
from tkinter import simpledialog

APP_DIR = os.path.join(
    os.environ.get("APPDATA", os.path.expanduser("~")), "ClipboardAgent"
)
CONFIG_PATH = os.path.join(APP_DIR, "config.json")


def load_config():
    """Devuelve {'server_url', 'token'} o None si no hay configuración.

    Prioridad: variables de entorno > config.json.
    """
    env_url = os.environ.get("CLIPBOARD_SERVER_URL")
    env_token = os.environ.get("CLIPBOARD_TOKEN")
    if env_url and env_token:
        return {"server_url": env_url, "token": env_token}

    if os.path.exists(CONFIG_PATH):
        try:
            with open(CONFIG_PATH, "r", encoding="utf-8") as f:
                data = json.load(f)
            if data.get("server_url") and data.get("token"):
                return data
        except (OSError, ValueError):
            pass
    return None


def save_config(config):
    os.makedirs(APP_DIR, exist_ok=True)
    with open(CONFIG_PATH, "w", encoding="utf-8") as f:
        json.dump(config, f, indent=2)


def setup_dialog():
    """Pide URL y token en un diálogo. Devuelve el config o None si se cancela."""
    root = tk.Tk()
    root.withdraw()
    try:
        url = simpledialog.askstring(
            "Clipboard — Configuración",
            "URL del servidor (ej. https://tu-app.vercel.app):",
            parent=root,
        )
        if not url:
            return None
        token = simpledialog.askstring(
            "Clipboard — Configuración",
            "Token del dispositivo (del panel /devices):",
            parent=root,
        )
        if not token:
            return None
        return {"server_url": url.strip().rstrip("/"), "token": token.strip()}
    finally:
        root.destroy()
