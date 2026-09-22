# Quita la tarea de arranque automático de ClipboardAgent.
param([string]$TaskName = "ClipboardAgent")
$ErrorActionPreference = 'Stop'

$existing = Get-ScheduledTask -TaskName $TaskName -ErrorAction SilentlyContinue
if ($null -eq $existing) {
  Write-Host "No existe la tarea '$TaskName'."
  return
}
Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
Write-Host "Tarea '$TaskName' eliminada."
