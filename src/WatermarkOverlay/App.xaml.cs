using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Threading;

namespace WatermarkOverlay
{
    public partial class App : System.Windows.Application
    {
        private Mutex? _singleInstanceMutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            bool created;
            _singleInstanceMutex = new Mutex(true, "Global/WatermarkOverlay_SingleInstance", out created);
            if (!created)
            {
                // Already running
                Shutdown();
                return;
            }
            // Hide main window immediately; we'll spawn overlay windows only.
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Watchdog: if overlay exits unexpectedly, relaunch
            AppDomain.CurrentDomain.UnhandledException += (_, __) => Relaunch();
            this.Exit += (_, __) => { };
        }

        private static void Relaunch()
        {
            try
            {
                var exe = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exe))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exe,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
            catch { }
        }
    }
}

