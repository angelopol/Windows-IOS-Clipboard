"""Arranque automático al iniciar sesión, vía Task Scheduler (schtasks).

Se auto-registra la primera vez que se ejecuta el .exe (build de PyInstaller).
No usa dependencias externas: llama a `schtasks`, que viene con Windows. Corre
en la sesión del usuario (`/IT`), por lo que el portapapeles, el hotkey y el
tray funcionan (a diferencia de un servicio en session 0).
"""
import subprocess
import sys

TASK_NAME = "ClipboardAgent"
_CREATE_NO_WINDOW = 0x08000000  # evita el parpadeo de una consola


def exe_path():
    """Ruta del .exe si estamos empaquetados con PyInstaller; si no, None.

    Solo tiene sentido auto-arrancar el ejecutable, no `python agent.py`.
    """
    if getattr(sys, "frozen", False):
        return sys.executable
    return None


def _run(args):
    return subprocess.run(
        args,
        capture_output=True,
        creationflags=_CREATE_NO_WINDOW,
    )


def is_enabled():
    try:
        return _run(["schtasks", "/Query", "/TN", TASK_NAME]).returncode == 0
    except Exception:
        return False


def enable():
    """Crea/actualiza la tarea de inicio de sesión. Devuelve True si lo logró."""
    exe = exe_path()
    if not exe:
        return False
    try:
        return (
            _run(
                [
                    "schtasks", "/Create",
                    "/TN", TASK_NAME,
                    "/TR", f'"{exe}"',
                    "/SC", "ONLOGON",
                    "/RL", "LIMITED",
                    "/IT",
                    "/F",
                ]
            ).returncode
            == 0
        )
    except Exception:
        return False


def disable():
    try:
        return _run(["schtasks", "/Delete", "/TN", TASK_NAME, "/F"]).returncode == 0
    except Exception:
        return False
