# Builds ClipboardAgent.exe with PyInstaller (local build).
# Requires Python 3.10+ on PATH.
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

python -m pip install --upgrade pip
pip install -r requirements.txt pyinstaller

pyinstaller --noconsole --onefile --name ClipboardAgent --hidden-import=pystray._win32 agent.py

Write-Host ""
Write-Host "Built: $PSScriptRoot\dist\ClipboardAgent.exe"
