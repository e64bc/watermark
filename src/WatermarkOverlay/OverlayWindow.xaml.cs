using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WatermarkOverlay
{
    public partial class OverlayWindow : Window
    {
        private readonly Screen _screen;
        private WatermarkConfig _config;

        public OverlayWindow(Screen targetScreen, WatermarkConfig config)
        {
            InitializeComponent();
            _screen = targetScreen;
            _config = config;

            // Position the overlay to the bounds of the monitor
            Left = _screen.Bounds.Left;
            Top = _screen.Bounds.Top;
            Width = _screen.Bounds.Width;
            Height = _screen.Bounds.Height;

            Loaded += (_, __) => MakeWindowClickThrough();
            Loaded += (_, __) => Render();
        }

        public void RefreshDynamicContent()
        {
            Render();
        }

        private void Render()
        {
            CanvasRoot.Children.Clear();

            string username = Environment.UserName;
            string now = DateTime.Now.ToString(_config.TimeFormat, CultureInfo.InvariantCulture);
            string text = BuildText(username, now);

            if (_config.Mode == DisplayMode.Corner)
            {
                DrawCorner(text);
            }
            else
            {
                DrawTiled(text);
            }
        }

        private string BuildText(string username, string now)
        {
            if (_config.ShowUsername && _config.ShowTime)
                return $"{username}  —  {now}";
            if (_config.ShowUsername)
                return username;
            if (_config.ShowTime)
                return now;
            return _config.FallbackText ?? "";
        }

        private void DrawCorner(string text)
        {
            var tb = CreateText(text);
            tb.Opacity = _config.Opacity;

            var margin = 12.0;
            double x = 0, y = 0;
            switch (_config.Corner)
            {
                case Corner.TopLeft:
                    x = margin; y = margin; break;
                case Corner.TopRight:
                    x = Width - margin; y = margin; tb.TextAlignment = TextAlignment.Right; break;
                case Corner.BottomLeft:
                    x = margin; y = Height - margin; tb.TextAlignment = TextAlignment.Left; break;
                case Corner.BottomRight:
                    x = Width - margin; y = Height - margin; tb.TextAlignment = TextAlignment.Right; break;
            }
            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            CanvasRoot.Children.Add(tb);
        }

        private void DrawTiled(string text)
        {
            double step = _config.TileStep <= 0 ? 320 : _config.TileStep;
            double angle = _config.TileAngleDegrees;

            var brush = new VisualBrush(new TextBlock
            {
                Text = text,
                FontSize = _config.FontSize,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(_config.Color),
                Opacity = _config.Opacity,
                LayoutTransform = new RotateTransform(angle)
            })
            {
                TileMode = TileMode.Tile,
                ViewportUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, step, step),
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center
            };

            var rect = new System.Windows.Shapes.Rectangle
            {
                Width = Width,
                Height = Height,
                Fill = brush,
                IsHitTestVisible = false
            };
            CanvasRoot.Children.Add(rect);
        }

        private TextBlock CreateText(string text)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = _config.FontSize,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(_config.Color),
                Background = System.Windows.Media.Brushes.Transparent,
                IsHitTestVisible = false
            };
        }

        private void MakeWindowClickThrough()
        {
            var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            int exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            exStyle |= WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE;
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}

