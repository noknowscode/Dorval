<#
.SYNOPSIS
  Stops any running instance, rebuilds the desktop head, launches it and captures a screenshot.

.NOTES
  Runs the managed dll rather than the exe so console logging is capturable — a windowed
  Uno desktop process writes nothing to a redirected stdout when launched as an exe.
#>
param(
  [string]$Out = 'run.png',
  [switch]$SkipBuild,
  [int]$SettleSeconds = 12,
  [switch]$FreshUser
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$bin = Join-Path $root 'Dorval\bin\Debug\net10.0-desktop'
$scratch = Split-Path -Parent $Out
if (-not $scratch) { $scratch = $bin }

# Only the app itself: matching any dotnet.exe mentioning Dorval also kills MSBuild,
# which leaves the intermediate output half-written.
Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe' OR Name = 'Dorval.exe'" |
  Where-Object { $_.Name -eq 'Dorval.exe' -or $_.CommandLine -match 'Dorval\.dll"?\s*$' } |
  ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
Start-Sleep -Milliseconds 600

if (-not $SkipBuild) {
  # Narrow TargetFrameworks rather than passing -f: -f still evaluates every head,
  # which needs the android/ios/wasm workloads restored just to build desktop.
  & dotnet build (Join-Path $root 'Dorval\Dorval.csproj') -p:TargetFrameworks=net10.0-desktop -v q -nologo
  if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}

if ($FreshUser) {
  # Clear the onboarding flag so the first-run flow can be checked again.
  $settings = Join-Path $env:LOCALAPPDATA 'Dorval\LocalState\.settings'
  Remove-Item $settings -Recurse -Force -ErrorAction SilentlyContinue
}

$env:APP_NO_HOTDESIGN = '1'
Push-Location $bin
$proc = Start-Process -FilePath 'dotnet' -ArgumentList 'Dorval.dll' `
  -RedirectStandardOutput (Join-Path $scratch 'app-out.log') `
  -RedirectStandardError (Join-Path $scratch 'app-err.log') -PassThru
Pop-Location

Start-Sleep -Seconds $SettleSeconds
& (Join-Path $PSScriptRoot 'Capture-Window.ps1') -ProcessId $proc.Id -Out $Out
Write-Output "PID=$($proc.Id)"
