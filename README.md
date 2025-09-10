## WatermarkOverlay

Always-on, click-through watermark overlay for all monitors (Windows, WPF).

### Features
- Always-on transparent, click-through overlays per monitor
- Two modes: Corner label or Tiled across the screen
- Shows logged-in username and/or time
- Configurable font size, opacity, color, corner, angle, and spacing
- MSI installer with silent install support

### Build prerequisites (Windows)
- .NET SDK 8.0+
- Visual Studio 2022 (Desktop development with .NET) or `dotnet` CLI
- WiX Toolset 3.11+ and WiX Visual Studio build targets (for MSI)

### Build app
```bash
dotnet build "src/WatermarkOverlay/WatermarkOverlay.csproj" -c Release
```

Artifacts:
- `src/WatermarkOverlay/bin/Release/net8.0-windows/WatermarkOverlay.exe`

### Configure
Edit `appsettings.json` next to the `WatermarkOverlay.exe`:
```json
{
  "Mode": "Tiled",                         // Corner | Tiled
  "Corner": "TopRight",                    // TopLeft | TopRight | BottomLeft | BottomRight
  "ShowUsername": true,
  "ShowTime": true,
  "TimeFormat": "yyyy-MM-dd HH:mm",
  "FallbackText": null,
  "FontSize": 28,
  "Opacity": 0.18,                         // 0.0 - 1.0
  "Color": "#40FF0000",                   // ARGB hex
  "TileAngleDegrees": -30,
  "TileStep": 360
}
```

### Build MSI
Open a Developer Command Prompt for VS (with MSBuild on PATH), and ensure WiX is installed (candle/light targets).
```bash
msbuild "WatermarkOverlay.sln" /p:Configuration=Release
```

Artifact:
- `installer/WiX/bin/Release/WatermarkOverlay.msi`

### Silent install
```bash
msiexec /i installer\WiX\bin\Release\WatermarkOverlay.msi /qn
```

Silent uninstall:
```bash
msiexec /x installer\WiX\bin\Release\WatermarkOverlay.msi /qn
```

### Notes
- This app is a plain WPF executable and WiX MSI; it avoids py2exe or similar packagers.
- The overlay uses layered, transparent, non-activated windows to remain click-through and non-blocking.

