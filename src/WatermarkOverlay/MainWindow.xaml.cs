using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Newtonsoft.Json;

namespace WatermarkOverlay
{
    public partial class MainWindow : Window
    {
        private readonly List<OverlayWindow> _overlays = new();
        private readonly System.Timers.Timer _tickTimer = new(1000);
        private readonly System.IO.FileSystemWatcher _cfgWatcher = new();

        public MainWindow()
        {
            InitializeComponent();

            var config = WatermarkConfig.Load();

            foreach (var screen in Screen.AllScreens)
            {
                var overlay = new OverlayWindow(screen, config);
                overlay.Show();
                _overlays.Add(overlay);
            }

            _tickTimer.Elapsed += (_, __) => Dispatcher.Invoke(() =>
            {
                foreach (var overlay in _overlays)
                {
                    overlay.RefreshDynamicContent();
                }
            });
            _tickTimer.Start();

            // Watch ProgramData config for changes
            try {
                var cfgPath = WatermarkConfig.ProgramDataConfigPath;
                var dir = System.IO.Path.GetDirectoryName(cfgPath);
                var file = System.IO.Path.GetFileName(cfgPath);
                if (!string.IsNullOrEmpty(dir) && !string.IsNullOrEmpty(file))
                {
                    _cfgWatcher.Path = dir;
                    _cfgWatcher.Filter = file;
                    _cfgWatcher.NotifyFilter = System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.Size | System.IO.NotifyFilters.FileName;
                    _cfgWatcher.Changed += (_, __) => DebouncedReload();
                    _cfgWatcher.Created += (_, __) => DebouncedReload();
                    _cfgWatcher.Renamed += (_, __) => DebouncedReload();
                    _cfgWatcher.EnableRaisingEvents = true;
                }
            } catch { }

            // Exit when user closes from tray, etc. For now, close to exit.
            this.Loaded += (_, __) => this.Hide();
        }

        private DateTime _lastReload = DateTime.MinValue;
        private void DebouncedReload()
        {
            var now = DateTime.UtcNow;
            if ((now - _lastReload).TotalMilliseconds < 500) return;
            _lastReload = now;
            Dispatcher.Invoke(() =>
            {
                var cfg = WatermarkConfig.Load();
                foreach (var overlay in _overlays)
                {
                    overlay.SetConfig(cfg);
                    overlay.RefreshDynamicContent();
                }
            });
        }
    }
}

