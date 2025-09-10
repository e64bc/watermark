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

            // Exit when user closes from tray, etc. For now, close to exit.
            this.Loaded += (_, __) => this.Hide();
        }
    }
}

