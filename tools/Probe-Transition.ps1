<#
.SYNOPSIS
  Films a tab transition with rapid PrintWindow captures and reports what each band of
  the window was doing frame by frame.

.DESCRIPTION
  Synthesized input cannot reach an Uno desktop window from a background process, so the
  app presses its own tab via DORVAL_NAV_PROBE. Filming starts -Lead ms before that press
  so the navigation lands early in the film and the frames after it are the bulk of it.

  The flash we are hunting is a frame where the content area goes paper-coloured because
  the outgoing page has been torn out and the incoming one is not up yet. Each frame is
  scored by the share of its content rows sitting on the paper colour; a spike is a flash.

.NOTES
  Requires a Debug build (DORVAL_NAV_PROBE is compiled out of Release).
#>
param(
  [string]$Route = 'Places',
  [int]$NavAt = 6000,
  [int]$Lead = 600,
  [int]$Frames = 80,
  [string]$Tag = 'probe',
  [string]$OutDir,
  # A full DORVAL_NAV_PROBE string, for filming a *revisit* rather than a first visit.
  # Its delays are cumulative ("Places@4000,Today@1500,Places@1500" presses at 4.0s, 5.5s
  # and 7.0s), so set -NavAt to the absolute time of the press you want in frame.
  [string]$Probe
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$bin = Join-Path $root 'Dorval\bin\Debug\net10.0-desktop'
if (-not $OutDir) { $OutDir = $bin }
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

# The two colours that tell the story: the paper the shell paints when nothing covers it,
# and the deep green of the persistent tab bar.
$paper = @(245, 243, 236)
$green = @(30, 75, 50)

Add-Type -AssemblyName System.Drawing
if (-not ([System.Management.Automation.PSTypeName]'ProbeNative').Type) {
  Add-Type -Namespace '' -Name ProbeNative -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool PrintWindow(System.IntPtr hwnd, System.IntPtr hdc, uint flags);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool GetWindowRect(System.IntPtr hwnd, out RECT rect);
public struct RECT { public int Left, Top, Right, Bottom; }
'@
}

Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
  Where-Object { $_.CommandLine -match 'Dorval\.dll' } |
  ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
Start-Sleep -Milliseconds 600

Remove-Item (Join-Path $env:TEMP 'dorval-route.log') -ErrorAction SilentlyContinue
$env:APP_NO_HOTDESIGN = '1'
$env:DORVAL_NAV_PROBE = if ($Probe) { $Probe } else { "$Route@$NavAt" }
Push-Location $bin
$proc = Start-Process -FilePath 'dotnet' -ArgumentList 'Dorval.dll' -PassThru
Pop-Location
Remove-Item env:DORVAL_NAV_PROBE

# The window can take ten seconds to appear, so wait for it rather than guessing.
$launched = [Diagnostics.Stopwatch]::StartNew()
$hwnd = [IntPtr]::Zero
while ($launched.Elapsed.TotalSeconds -lt 40) {
  $proc.Refresh()
  $hwnd = $proc.MainWindowHandle
  if ($hwnd -ne [IntPtr]::Zero) { break }
  Start-Sleep -Milliseconds 200
}
if ($hwnd -eq [IntPtr]::Zero) { throw 'No window after 40s.' }

# The probe's clock starts when the app writes "ready", not when the window appears — a
# cold start can put ten seconds between the two, which is what mis-centred the first
# film. Wait for the marker, then count the lead from there.
$trace = Join-Path $env:TEMP 'dorval-route.log'
$waited = [Diagnostics.Stopwatch]::StartNew()
while ($waited.Elapsed.TotalSeconds -lt 40) {
  if ((Test-Path $trace) -and ((Get-Content $trace -Raw) -match 'ready')) { break }
  Start-Sleep -Milliseconds 100
}
$readyAt = [Diagnostics.Stopwatch]::StartNew()
Write-Output "window+ready after $([int]$launched.Elapsed.TotalMilliseconds)ms"

$sleep = $NavAt - $Lead - [int]$readyAt.Elapsed.TotalMilliseconds
if ($sleep -gt 0) { Start-Sleep -Milliseconds $sleep }

$rect = New-Object ProbeNative+RECT
[void][ProbeNative]::GetWindowRect($hwnd, [ref]$rect)
$w = $rect.Right - $rect.Left
$h = $rect.Bottom - $rect.Top

$shots = @()
$sw = [Diagnostics.Stopwatch]::StartNew()
for ($i = 0; $i -lt $Frames; $i++) {
  $bmp = New-Object System.Drawing.Bitmap $w, $h
  $gfx = [System.Drawing.Graphics]::FromImage($bmp)
  $hdc = $gfx.GetHdc()
  [void][ProbeNative]::PrintWindow($hwnd, $hdc, 2)
  $gfx.ReleaseHdc($hdc); $gfx.Dispose()
  $shots += [pscustomobject]@{ Ms = [int]$sw.Elapsed.TotalMilliseconds; Bmp = $bmp }
}
$sw.Stop()
Write-Output "filmed $Frames frames over $([int]$sw.Elapsed.TotalMilliseconds)ms; nav pressed at ~$($Lead)ms"

function Test-Near([System.Drawing.Color]$c, [int[]]$target, [int]$tol = 6) {
  return ([Math]::Abs($c.R - $target[0]) -le $tol) -and
         ([Math]::Abs($c.G - $target[1]) -le $tol) -and
         ([Math]::Abs($c.B - $target[2]) -le $tol)
}

# Content runs from below the title bar to just above the tab bar. Score each frame by
# how much of that strip is bare paper — one number per frame, so a flash is a spike.
$x = [int]($w / 2)
$contentTop = 40
$contentBottom = $h - 90
$rows = $contentTop..$contentBottom | Where-Object { ($_ - $contentTop) % 4 -eq 0 }

$lines = foreach ($shot in $shots) {
  $bare = 0
  foreach ($y in $rows) {
    if (Test-Near $shot.Bmp.GetPixel($x, $y) $paper) { $bare++ }
  }
  $pct = [int](100 * $bare / $rows.Count)
  $top = $shot.Bmp.GetPixel($x, 60)
  $bar = $shot.Bmp.GetPixel($x, $h - 40)
  $barOk = if (Test-Near $bar $green 10) { 'green' } else { "$($bar.R),$($bar.G),$($bar.B)" }
  "t={0,5}ms  paper={1,3}%  header={2},{3},{4}  bar={5}" -f $shot.Ms, $pct, $top.R, $top.G, $top.B, $barOk
}
$lines

# Keep the frames around the swap for eyeballing.
$keep = 0
foreach ($shot in $shots) {
  if ($keep -lt 14 -and $shot.Ms -ge ($Lead - 60)) {
    $shot.Bmp.Save((Join-Path $OutDir "$Tag-$($shot.Ms)ms.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    $keep++
  }
  $shot.Bmp.Dispose()
}
Write-Output "--- route log ---"
Get-Content $trace -ErrorAction SilentlyContinue
Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
