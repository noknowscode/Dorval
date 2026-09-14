<#
.SYNOPSIS
  Captures the Dorval window to a PNG.

.DESCRIPTION
  Uses PrintWindow with PW_RENDERFULLCONTENT, which is occlusion-proof for the Skia
  GL window. PrintWindow can return TRUE and still hand back a blank bitmap, so the
  result is sampled; if it is effectively uniform, the window is foregrounded and
  CopyFromScreen is used instead.
#>
param(
  [Parameter(Mandatory = $true)][int]$ProcessId,
  [Parameter(Mandatory = $true)][string]$Out
)

Add-Type -AssemblyName System.Drawing

if (-not ([System.Management.Automation.PSTypeName]'DorvalNative').Type) {
  Add-Type -Namespace '' -Name DorvalNative -MemberDefinition @'
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool PrintWindow(System.IntPtr hwnd, System.IntPtr hdc, uint flags);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool GetWindowRect(System.IntPtr hwnd, out RECT rect);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool SetForegroundWindow(System.IntPtr hwnd);
[System.Runtime.InteropServices.DllImport("user32.dll")]
public static extern bool IsIconic(System.IntPtr hwnd);
public struct RECT { public int Left, Top, Right, Bottom; }
'@
}

$proc = Get-Process -Id $ProcessId -ErrorAction Stop
$hwnd = $proc.MainWindowHandle
if ($hwnd -eq [IntPtr]::Zero) { throw "Process $ProcessId has no main window (handle 0)." }
if ([DorvalNative]::IsIconic($hwnd)) { throw 'Window is minimised; its rect would be a sliver.' }

$rect = New-Object DorvalNative+RECT
[void][DorvalNative]::GetWindowRect($hwnd, [ref]$rect)
$w = $rect.Right - $rect.Left
$h = $rect.Bottom - $rect.Top
if ($w -le 0 -or $h -le 0) { throw "Bad window rect ${w}x${h}." }

$bmp = New-Object System.Drawing.Bitmap $w, $h
$gfx = [System.Drawing.Graphics]::FromImage($bmp)
$hdc = $gfx.GetHdc()
[void][DorvalNative]::PrintWindow($hwnd, $hdc, 2)  # PW_RENDERFULLCONTENT
$gfx.ReleaseHdc($hdc)
$gfx.Dispose()

# A success return is not evidence of pixels: sample before trusting it.
$distinct = @{}
for ($y = 0; $y -lt $h; $y += [Math]::Max(1, [int]($h / 24))) {
  for ($x = 0; $x -lt $w; $x += [Math]::Max(1, [int]($w / 24))) {
    $distinct[$bmp.GetPixel($x, $y).ToArgb()] = $true
  }
}

if ($distinct.Count -lt 4) {
  Write-Output 'PrintWindow returned a near-uniform bitmap; falling back to screen copy.'
  [void][DorvalNative]::SetForegroundWindow($hwnd)
  Start-Sleep -Milliseconds 400
  $gfx = [System.Drawing.Graphics]::FromImage($bmp)
  $gfx.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bmp.Size)
  $gfx.Dispose()
}

$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$bmp.Dispose()
Write-Output "Saved $Out (${w}x${h}, $($distinct.Count) sampled colours)"
