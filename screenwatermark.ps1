# screenwatermark.ps1
# Always-on, click-through watermark overlay with SETTINGS for mode & content.

# ===================== SETTINGS =====================
# Mode: "Corner" for a single label in a corner, or "Tiled" to cover the screen.
$Mode = "Tiled"            # "Corner" or "Tiled"

# Show only on the primary display?
$ApplyToAllMonitors = $true # $true = all monitors, $false = primary only

# What to show:
$ShowUser     = $true
$ShowComputer = $false
$ShowTime     = $True
$TimeFormat   = "MM-dd HH:mm"  # used only if $ShowTime

# Corner placement (when $Mode = "Corner"):
$Corner = "TopRight"         # "TopRight","TopLeft","BottomRight","BottomLeft"
$MarginPx = 12               # distance from edges

# Visual style:
$FontFamily = "Segoe UI"
$FontSizePt = 36
$FontWeight = 'Bold'         # 'Normal','Bold','SemiBold', etc.
$Opacity    = 0.15           # 0.0..1.0 (higher = more visible)
# Text color (RGB):
$R = 255; $G = 0; $B = 0     # bright red

# Tiled mode options (used only when $Mode = "Tiled"):
$AngleDeg = -30              # rotation angle
$TileX    = 380              # horizontal spacing (px)
$TileY    = 220              # vertical spacing (px)

# Time refresh interval (minutes) if $ShowTime:
$UpdateIntervalMinutes = 1
# ====================================================

Add-Type -AssemblyName PresentationCore,PresentationFramework,WindowsBase,System.Windows.Forms

# Win32 interop to make window click-through
$win32 = @"
using System;
using System.Runtime.InteropServices;
public static class Win32 {
  [DllImport("user32.dll")] public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
  [DllImport("user32.dll")] public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
  public const int GWL_EXSTYLE = -20;
  public const int WS_EX_TRANSPARENT = 0x00000020;
  public const int WS_EX_LAYERED     = 0x00080000;
  public const int WS_EX_NOACTIVATE  = 0x08000000;
}
"@
Add-Type $win32

# ----- Brushes/Fonts -----
$alphaByte = [byte]([math]::Round(255 * [math]::Min([math]::Max($Opacity,0),1)))
$color     = [System.Windows.Media.Color]::FromArgb($alphaByte, [byte]$R, [byte]$G, [byte]$B)
$brush     = New-Object System.Windows.Media.SolidColorBrush $color
$font      = New-Object System.Windows.Media.FontFamily $FontFamily

# Gather target screens
$screens = if ($ApplyToAllMonitors) { [System.Windows.Forms.Screen]::AllScreens } else { @([System.Windows.Forms.Screen]::PrimaryScreen) }

# Keep references
$windows = @()
$textBlocks = @()  # includes all TextBlocks (1 per window in Corner mode; many per window in Tiled mode)

function Get-CurrentWatermarkText {
    $parts = @()
    if ($ShowUser)     { $parts += $env:USERNAME }
    if ($ShowComputer) { $parts += $env:COMPUTERNAME }
    if ($ShowTime)     { $parts += (Get-Date -Format $TimeFormat) }
    if ($parts.Count -eq 0) { return "" }
    # Join with " @ " between user and computer if both exist; otherwise space them nicely
    if ($ShowUser -and $ShowComputer) {
        $first = "$env:USERNAME @ $env:COMPUTERNAME"
        if ($ShowTime) { return "$first  $(Get-Date -Format $TimeFormat)" } else { return $first }
    } else {
        return ($parts -join "  ")
    }
}

function Add-ClickThrough($w) {
    $w.Add_Loaded({
        param($sender, $e)
        $ih = (New-Object System.Windows.Interop.WindowInteropHelper($sender)).Handle
        $ex = [Win32]::GetWindowLong($ih, [Win32]::GWL_EXSTYLE)
        $ex = $ex -bor [Win32]::WS_EX_TRANSPARENT -bor [Win32]::WS_EX_LAYERED -bor [Win32]::WS_EX_NOACTIVATE
        [Win32]::SetWindowLong($ih, [Win32]::GWL_EXSTYLE, $ex) | Out-Null
    })
}

function Build-CornerWindow($screen) {
    $w = New-Object System.Windows.Window
    $w.WindowStyle        = 'None'
    $w.AllowsTransparency = $true
    $w.Background         = [System.Windows.Media.Brushes]::Transparent
    $w.Topmost            = $true
    $w.ShowInTaskbar      = $false
    $w.Left   = $screen.Bounds.Left
    $w.Top    = $screen.Bounds.Top
    $w.Width  = $screen.Bounds.Width
    $w.Height = $screen.Bounds.Height

    $grid = New-Object System.Windows.Controls.Grid
    $w.Content = $grid

    $tb = New-Object System.Windows.Controls.TextBlock
    $tb.FontFamily = $font
    $tb.FontSize   = $FontSizePt
    $tb.FontWeight = $FontWeight
    $tb.Foreground = $brush
    $tb.Opacity    = 1.0

    switch ($Corner) {
        "TopLeft"     { $tb.HorizontalAlignment='Left'  ; $tb.VerticalAlignment='Top'    }
        "TopRight"    { $tb.HorizontalAlignment='Right' ; $tb.VerticalAlignment='Top'    }
        "BottomLeft"  { $tb.HorizontalAlignment='Left'  ; $tb.VerticalAlignment='Bottom' }
        "BottomRight" { $tb.HorizontalAlignment='Right' ; $tb.VerticalAlignment='Bottom' }
        default       { $tb.HorizontalAlignment='Right' ; $tb.VerticalAlignment='Top'    }
    }
    $tb.Margin = New-Object System.Windows.Thickness($MarginPx)

    [System.Windows.Media.TextOptions]::SetTextFormattingMode($tb, 'Display')
    [System.Windows.Media.TextOptions]::SetTextRenderingMode($tb, 'ClearType')

    [void]$grid.Children.Add($tb)

    Add-ClickThrough $w
    return @{ Window=$w; TextBlocks=@($tb) }
}

function Build-TiledWindow($screen) {
    $w = New-Object System.Windows.Window
    $w.WindowStyle        = 'None'
    $w.AllowsTransparency = $true
    $w.Background         = [System.Windows.Media.Brushes]::Transparent
    $w.Topmost            = $true
    $w.ShowInTaskbar      = $false
    $w.Left   = $screen.Bounds.Left
    $w.Top    = $screen.Bounds.Top
    $w.Width  = $screen.Bounds.Width
    $w.Height = $screen.Bounds.Height

    $canvas = New-Object System.Windows.Controls.Canvas
    $w.Content = $canvas

    # Build a bunch of text blocks tiled across
    $tbs = @()
    # Rough measurement for stepping
    $measure = New-Object System.Windows.Controls.TextBlock
    $measure.Text       = "MMMMMMMMMMMM"  # dummy
    $measure.FontFamily = $font
    $measure.FontSize   = $FontSizePt
    $measure.FontWeight = $FontWeight

    for ($y = -$TileY; $y -lt $w.Height + $TileY; $y += $TileY) {
        for ($x = -$TileX; $x -lt $w.Width + $TileX; $x += $TileX) {
            $tb = New-Object System.Windows.Controls.TextBlock
            $tb.FontFamily = $font
            $tb.FontSize   = $FontSizePt
            $tb.FontWeight = $FontWeight
            $tb.Foreground = $brush
            $tb.Opacity    = 1.0
            $tb.RenderTransform = (New-Object System.Windows.Media.RotateTransform($AngleDeg))
            $tb.RenderTransformOrigin = New-Object System.Windows.Point(0.5, 0.5)

            [System.Windows.Controls.Canvas]::SetLeft($tb, $x)
            [System.Windows.Controls.Canvas]::SetTop($tb, $y)
            [void]$canvas.Children.Add($tb)
            $tbs += $tb
        }
    }

    Add-ClickThrough $w
    return @{ Window=$w; TextBlocks=$tbs }
}

# Build windows per chosen mode
foreach ($screen in $screens) {
    $built = if ($Mode -ieq "Tiled") { Build-TiledWindow $screen } else { Build-CornerWindow $screen }
    $windows += $built.Window
    $textBlocks += $built.TextBlocks
}

function Update-WatermarkText {
    $text = Get-CurrentWatermarkText
    foreach ($tb in $textBlocks) {
        $tb.Text = $text
    }
}

# Show windows and set initial text
$windows | ForEach-Object { $_.Show() } | Out-Null
Update-WatermarkText

# Refresh time if enabled
if ($ShowTime -and $UpdateIntervalMinutes -gt 0) {
    $timer = New-Object System.Windows.Threading.DispatcherTimer
    $timer.Interval = [TimeSpan]::FromMinutes([double]$UpdateIntervalMinutes)
    $timer.Add_Tick({ Update-WatermarkText })
    $timer.Start()
}

# Keep process alive
$frame = New-Object System.Windows.Threading.DispatcherFrame
[System.Windows.Threading.Dispatcher]::PushFrame($frame)
