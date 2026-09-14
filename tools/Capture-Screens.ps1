<#
.SYNOPSIS
  Launches the app once per screen via DORVAL_START_ROUTE and captures each one.

.NOTES
  Synthesized pointer input cannot reliably drive an Uno desktop window from a
  background process, so each screen is reached by starting the app on it instead.
#>
param(
  [Parameter(Mandatory = $true)][string]$OutDir,
  [string[]]$Routes = @('Onboarding', 'Today', 'Discover', 'Places', 'Community', 'Saved', 'Event:cinema', 'Reminder', 'Post:swim', 'Safety'),
  [switch]$SkipBuild,
  [int]$SettleSeconds = 11
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$bin = Join-Path $root 'Dorval\bin\Debug\net10.0-desktop'
New-Item -ItemType Directory -Force $OutDir | Out-Null

function Stop-App {
  Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe' OR Name = 'Dorval.exe'" |
    Where-Object { $_.Name -eq 'Dorval.exe' -or $_.CommandLine -match 'Dorval\.dll"?\s*$' } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
  Start-Sleep -Milliseconds 600
}

# A previous run still holds the output assembly.
Stop-App

if (-not $SkipBuild) {
  & dotnet build (Join-Path $root 'Dorval\Dorval.csproj') -p:TargetFrameworks=net10.0-desktop -v q -nologo
  if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}

$env:APP_NO_HOTDESIGN = '1'
$index = 0

foreach ($route in $Routes) {
  $index++
  Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe' OR Name = 'Dorval.exe'" |
    Where-Object { $_.Name -eq 'Dorval.exe' -or $_.CommandLine -match 'Dorval\.dll"?\s*$' } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
  Start-Sleep -Milliseconds 500

  if ($route -eq 'Onboarding') {
    Remove-Item (Join-Path $env:LOCALAPPDATA 'O=Ville de Dorval\com.dorval.cheznous\LocalState') -Recurse -Force -ErrorAction SilentlyContinue
  }

  $env:DORVAL_START_ROUTE = $route
  Push-Location $bin
  $proc = Start-Process -FilePath 'dotnet' -ArgumentList 'Dorval.dll' `
    -RedirectStandardOutput (Join-Path $OutDir 'app-out.log') `
    -RedirectStandardError (Join-Path $OutDir 'app-err.log') -PassThru
  Pop-Location

  Start-Sleep -Seconds $SettleSeconds

  $name = ('{0:d2}-{1}.png' -f $index, ($route -replace ':', '-'))
  try {
    & (Join-Path $PSScriptRoot 'Capture-Window.ps1') -ProcessId $proc.Id -Out (Join-Path $OutDir $name)
  }
  catch {
    Write-Output "FAILED $route : $_"
  }
}

Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe' OR Name = 'Dorval.exe'" |
  Where-Object { $_.Name -eq 'Dorval.exe' -or $_.CommandLine -match 'Dorval\.dll"?\s*$' } |
  ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
