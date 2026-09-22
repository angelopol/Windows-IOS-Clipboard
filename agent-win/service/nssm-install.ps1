# Instala ClipboardAgent como servicio de Windows con NSSM.
#
# ============================ ADVERTENCIA ============================
# Un servicio corre en SESSION 0, aislado del escritorio del usuario. En
# Windows 10/11 eso significa que el agente NO podrá:
#   - leer el portapapeles del usuario (auto-envío al copiar),
#   - recibir el hotkey global Ctrl+Alt+V,
#   - mostrar el icono de bandeja ni el menú de selección.
# El servicio arrancará, pero sus funciones interactivas quedarán inertes.
# Para uso real usa install-autostart.ps1 (Task Scheduler al iniciar sesión).
# Este script se incluye solo porque se pidió NSSM explícitamente.
# =====================================================================
#
# Requiere NSSM en el PATH (https://nssm.cc  o  'choco install nssm').
# La config debe ir por variables de entorno (un servicio no puede mostrar el
# diálogo de configuración), así que pasa -ServerUrl y -Token.
#
# Uso:
#   .\nssm-install.ps1 -ServerUrl "https://tu-app.vercel.app" -Token "clip_XXXX"

param(
  [string]$ExePath   = "$PSScriptRoot\..\dist\ClipboardAgent.exe",
  [string]$ServiceName = "ClipboardAgent",
  [Parameter(Mandatory = $true)][string]$ServerUrl,
  [Parameter(Mandatory = $true)][string]$Token,
  [string]$Nssm = "nssm"
)
$ErrorActionPreference = 'Stop'

if (-not (Get-Command $Nssm -ErrorAction SilentlyContinue)) {
  throw "No se encuentra NSSM en el PATH. Instálalo (choco install nssm) o pasa -Nssm con la ruta."
}
if (-not (Test-Path $ExePath)) {
  throw "No se encuentra el ejecutable: $ExePath. Compílalo con build.ps1 o pasa -ExePath."
}
$ExePath = (Resolve-Path $ExePath).Path

Write-Warning "Recuerda: como servicio (session 0) el portapapeles, el hotkey y el tray NO funcionarán."

& $Nssm install $ServiceName $ExePath
& $Nssm set $ServiceName AppEnvironmentExtra "CLIPBOARD_SERVER_URL=$ServerUrl" "CLIPBOARD_TOKEN=$Token"
& $Nssm set $ServiceName Start SERVICE_AUTO_START
& $Nssm set $ServiceName AppStdout "$env:APPDATA\ClipboardAgent\service.log"
& $Nssm set $ServiceName AppStderr "$env:APPDATA\ClipboardAgent\service.log"
& $Nssm start $ServiceName

Write-Host "Servicio '$ServiceName' instalado y arrancado (con las limitaciones indicadas)."
