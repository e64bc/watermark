using System;
using System.IO;
using System.Text.Json;
using WatermarkOverlay;

internal class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var cfgPath = WatermarkConfig.ProgramDataConfigPath;
            var dir = Path.GetDirectoryName(cfgPath)!;
            Directory.CreateDirectory(dir);

            var cfg = File.Exists(cfgPath)
                ? JsonSerializer.Deserialize<WatermarkConfigOnDisk>(File.ReadAllText(cfgPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                  ?? new WatermarkConfigOnDisk()
                : new WatermarkConfigOnDisk();

            // Simple args: key=value pairs
            foreach (var arg in args)
            {
                var parts = arg.Split('=', 2);
                if (parts.Length != 2) continue;
                var k = parts[0].Trim();
                var v = parts[1].Trim();
                switch (k.ToLowerInvariant())
                {
                    case "mode": cfg.Mode = v; break;
                    case "corner": cfg.Corner = v; break;
                    case "showusername": cfg.ShowUsername = v.Equals("true", StringComparison.OrdinalIgnoreCase); break;
                    case "showtime": cfg.ShowTime = v.Equals("true", StringComparison.OrdinalIgnoreCase); break;
                    case "timeformat": cfg.TimeFormat = v; break;
                    case "fallbacktext": cfg.FallbackText = string.IsNullOrWhiteSpace(v) ? null : v; break;
                    case "fontsize": if (double.TryParse(v, out var fs)) cfg.FontSize = fs; break;
                    case "opacity": if (double.TryParse(v, out var op)) cfg.Opacity = op; break;
                    case "color": cfg.Color = v; break;
                    case "tileangledegrees": if (double.TryParse(v, out var ang)) cfg.TileAngleDegrees = ang; break;
                    case "tilestep": if (double.TryParse(v, out var step)) cfg.TileStep = step; break;
                }
            }

            File.WriteAllText(cfgPath, JsonSerializer.Serialize(cfg, new JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
            return 1;
        }
    }
}

