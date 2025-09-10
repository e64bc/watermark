using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace WatermarkOverlay
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Hide main window immediately; we'll spawn overlay windows only.
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }
    }
}

