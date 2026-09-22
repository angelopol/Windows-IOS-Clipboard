[English](DEPLOY.md) · **Español**

# Compilar, publicar y auto-arrancar el agente de Windows

## 1. Compilar el `.exe`

### Opción A — GitHub Actions (recomendada)
El workflow [`.github/workflows/release.yml`](../.github/workflows/release.yml)
compila `ClipboardAgent.exe` en un runner de Windows y lo adjunta a un Release de
GitHub cuando haces push de un tag. No necesita Python local.

```bash
git tag v0.1.0
git push origin v0.1.0
```

El release aparece en `https://github.com/<usuario>/<repo>/releases` con el
`.exe` adjunto. También puedes lanzar el workflow a mano (workflow_dispatch) para
obtener el `.exe` como artefacto sin crear un release.

### Opción B — Compilación local
Requiere Python 3.10+ en el PATH:

```powershell
cd agent-win
.\build.ps1
# -> dist\ClipboardAgent.exe
```

## 2. Configurar

Antes del arranque automático, el agente necesita la URL del servidor y un token
de dispositivo (del panel `/devices`). Puedes:

- Ejecutar `ClipboardAgent.exe` una vez — un diálogo pide ambos y los guarda en
  `%APPDATA%\ClipboardAgent\config.json`; o
- Definir las variables de entorno `CLIPBOARD_SERVER_URL` y `CLIPBOARD_TOKEN`
  (tienen prioridad).

## 3. Arranque automático

### Recomendado — Task Scheduler al iniciar sesión
Corre en tu sesión de usuario, así que el portapapeles, el hotkey `Ctrl+Alt+V` y
el icono de bandeja funcionan.

```powershell
cd agent-win\service
.\install-autostart.ps1            # usa ..\dist\ClipboardAgent.exe
# desinstalar:  .\uninstall-autostart.ps1
```

### Bajo petición — Servicio con NSSM (con gran advertencia)

> ⚠️ Un servicio de Windows corre en **session 0**, aislado de tu escritorio. En
> Windows 10/11 el agente entonces **no puede** leer tu portapapeles, recibir el
> hotkey global ni mostrar el icono de bandeja. El servicio arranca, pero sus
> funciones interactivas no hacen nada. Prefiere Task Scheduler. Se incluye
> porque NSSM se pidió explícitamente.

```powershell
# necesita NSSM en el PATH (choco install nssm)
cd agent-win\service
.\nssm-install.ps1 -ServerUrl "https://tu-app.vercel.app" -Token "clip_XXXX"
# desinstalar:  .\nssm-uninstall.ps1
```

El script de NSSM pasa la config por variables de entorno (un servicio no puede
mostrar el diálogo de configuración) y escribe logs en
`%APPDATA%\ClipboardAgent\service.log`.

## Por qué un servicio no encaja con este agente

El agente es una app interactiva de escritorio: vigila el portapapeles de la
**sesión de usuario**, captura un **hotkey global**, muestra un **icono de
bandeja** y una ventana de selección. Todo eso requiere la sesión del usuario
conectado, que un servicio en session 0 no tiene. "Siempre encendido y arranca
solo" se logra mejor con Task Scheduler al iniciar sesión — es el despliegue
previsto.
