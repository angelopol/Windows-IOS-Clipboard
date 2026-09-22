# Desinstala el servicio ClipboardAgent creado con NSSM.
param(
  [string]$ServiceName = "ClipboardAgent",
  [string]$Nssm = "nssm"
)
$ErrorActionPreference = 'Stop'

if (-not (Get-Command $Nssm -ErrorAction SilentlyContinue)) {
  throw "No se encuentra NSSM en el PATH."
}
& $Nssm stop $ServiceName
& $Nssm remove $ServiceName confirm
Write-Host "Servicio '$ServiceName' eliminado."
