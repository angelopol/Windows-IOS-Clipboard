# Registra ClipboardAgent para arrancar al iniciar sesión (Task Scheduler).
# Esta es la forma RECOMENDADA: corre en tu sesión de usuario, así que el
# portapapeles, el hotkey Ctrl+Alt+V y el icono de bandeja funcionan.
#
# Uso:
#   .\install-autostart.ps1                         # usa ..\dist\ClipboardAgent.exe
#   .\install-autostart.ps1 -ExePath "C:\ruta\ClipboardAgent.exe"
#
# Antes de habilitarlo, ejecuta el .exe una vez a mano para configurar la URL y
# el token (se guardan en %APPDATA%\ClipboardAgent\config.json), o define las
# variables de entorno CLIPBOARD_SERVER_URL y CLIPBOARD_TOKEN.

param(
  [string]$ExePath = "$PSScriptRoot\..\dist\ClipboardAgent.exe",
  [string]$TaskName = "ClipboardAgent"
)
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $ExePath)) {
  throw "No se encuentra el ejecutable: $ExePath. Compílalo con build.ps1 o pasa -ExePath."
}
$ExePath = (Resolve-Path $ExePath).Path

$action = New-ScheduledTaskAction -Execute $ExePath
$trigger = New-ScheduledTaskTrigger -AtLogOn
$principal = New-ScheduledTaskPrincipal -UserId $env:USERNAME -LogonType Interactive -RunLevel Limited
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -ExecutionTimeLimit ([TimeSpan]::Zero)

Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null

Write-Host "Tarea '$TaskName' registrada. Arrancará al iniciar sesión."
Write-Host "Para probar ahora:  Start-ScheduledTask -TaskName $TaskName"
