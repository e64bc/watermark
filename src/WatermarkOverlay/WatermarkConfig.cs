using System;
using System.IO;
using System.Windows.Media;
using Newtonsoft.Json;

namespace WatermarkOverlay
{
    public enum DisplayMode { Corner, Tiled }
    public enum Corner { TopLeft, TopRight, BottomLeft, BottomRight }

    public class WatermarkConfig
    {
        public DisplayMode Mode { get; set; } = DisplayMode.Tiled;
        public Corner Corner { get; set; } = Corner.TopRight;
        public bool ShowUsername { get; set; } = true;
        public bool ShowTime { get; set; } = true;
        public string TimeFormat { get; set; } = "MM-dd HH:mm";
        public string? FallbackText { get; set; } = null;

        public double FontSize { get; set; } = 20;
        public double Opacity { get; set; } = 0.18;
        public System.Windows.Media.Color Color { get; set; } = System.Windows.Media.Color.FromArgb(255, 200, 0, 0);

        public double TileAngleDegrees { get; set; } = -30;
        public double TileStep { get; set; } = 360;

        public static string ConfigPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        public static string UserConfigPath
        {
            get
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var dir = Path.Combine(appData, "WatermarkOverlay");
                try { Directory.CreateDirectory(dir); } catch { }
                return Path.Combine(dir, "appsettings.json");
            }
        }

        public static WatermarkConfig Load()
        {
            try
            {
                if (File.Exists(UserConfigPath))
                {
                    var textUser = File.ReadAllText(UserConfigPath);
                    var cfgUser = JsonConvert.DeserializeObject<WatermarkConfigOnDisk>(textUser);
                    return cfgUser?.ToConfig() ?? new WatermarkConfig();
                }
                if (File.Exists(ConfigPath))
                {
                    var text = File.ReadAllText(ConfigPath);
                    var cfg = JsonConvert.DeserializeObject<WatermarkConfigOnDisk>(text);
                    return cfg?.ToConfig() ?? new WatermarkConfig();
                }
            }
            catch { }
            return new WatermarkConfig();
        }
    }

    // Helper to serialize Color as hex
    internal class WatermarkConfigOnDisk
    {
        public string Mode { get; set; } = "Tiled";
        public string Corner { get; set; } = "TopRight";
        public bool ShowUsername { get; set; } = true;
        public bool ShowTime { get; set; } = true;
        public string TimeFormat { get; set; } = "MM-dd HH:mm";
        public string? FallbackText { get; set; } = null;
        public double FontSize { get; set; } = 28;
        public double Opacity { get; set; } = 0.2;
        public string Color { get; set; } = "#FFC80000";
        public double TileAngleDegrees { get; set; } = -30;
        public double TileStep { get; set; } = 360;

        public WatermarkConfig ToConfig()
        {
            return new WatermarkConfig
            {
                Mode = Enum.TryParse<DisplayMode>(Mode, true, out var m) ? m : DisplayMode.Tiled,
                Corner = Enum.TryParse<Corner>(Corner, true, out var c) ? c : WatermarkOverlay.Corner.TopRight,
                ShowUsername = ShowUsername,
                ShowTime = ShowTime,
                TimeFormat = TimeFormat,
                FallbackText = FallbackText,
                FontSize = FontSize,
                Opacity = Opacity,
                Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Color)!,
                TileAngleDegrees = TileAngleDegrees,
                TileStep = TileStep
            };
        }
    }
}

