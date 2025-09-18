using System;
using System.IO;
using System.Windows.Media;
using Newtonsoft.Json;
using Microsoft.Win32;

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
        public static string ProgramDataConfigPath
        {
            get
            {
                var commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var dir = Path.Combine(commonAppData, "WatermarkOverlay");
                return Path.Combine(dir, "appsettings.json");
            }
        }

        public static WatermarkConfig Load()
        {
            try
            {
                var fromPolicy = LoadFromPolicy();
                if (fromPolicy != null) return fromPolicy;
                if (File.Exists(ProgramDataConfigPath))
                {
                    var textPd = File.ReadAllText(ProgramDataConfigPath);
                    var cfgPd = JsonConvert.DeserializeObject<WatermarkConfigOnDisk>(textPd);
                    return cfgPd?.ToConfig() ?? new WatermarkConfig();
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

        private static WatermarkConfig? LoadFromPolicy()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey("Software\\Policies\\WatermarkOverlay");
                if (key == null) return null;
                var cfg = new WatermarkConfig();
                cfg.Mode = Enum.TryParse<DisplayMode>(key.GetValue("Mode") as string, true, out var m) ? m : cfg.Mode;
                cfg.Corner = Enum.TryParse<Corner>(key.GetValue("Corner") as string, true, out var c) ? c : cfg.Corner;
                cfg.ShowUsername = GetBool(key, "ShowUsername", cfg.ShowUsername);
                cfg.ShowTime = GetBool(key, "ShowTime", cfg.ShowTime);
                cfg.TimeFormat = (key.GetValue("TimeFormat") as string) ?? cfg.TimeFormat;
                cfg.FallbackText = key.GetValue("FallbackText") as string ?? cfg.FallbackText;
                cfg.FontSize = GetDouble(key, "FontSize", cfg.FontSize);
                cfg.Opacity = GetDouble(key, "Opacity", cfg.Opacity);
                var colorStr = key.GetValue("Color") as string;
                if (!string.IsNullOrWhiteSpace(colorStr))
                    cfg.Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorStr)!;
                cfg.TileAngleDegrees = GetDouble(key, "TileAngleDegrees", cfg.TileAngleDegrees);
                cfg.TileStep = GetDouble(key, "TileStep", cfg.TileStep);
                return cfg;
            }
            catch { return null; }
        }

        private static bool GetBool(RegistryKey key, string name, bool defaultValue)
        {
            try
            {
                var v = key.GetValue(name);
                if (v is int i) return i != 0;
                if (v is string s && bool.TryParse(s, out var b)) return b;
            }
            catch { }
            return defaultValue;
        }

        private static double GetDouble(RegistryKey key, string name, double def)
        {
            try
            {
                var v = key.GetValue(name);
                if (v is int i) return i;
                if (v is string s && double.TryParse(s, out var d)) return d;
            }
            catch { }
            return def;
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

