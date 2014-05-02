using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using Common.Native;

namespace FloatingStatusWindowLibrary
{
    internal partial class MainWindow
    {
        private const int WindowCaptionHeight = 24;

        private readonly WindowChrome _windowChrome;

        private WindowSettings _windowSettings;
        public WindowSettings WindowSettings
        {
            get { return _windowSettings; }
            set { _windowSettings = value; }
        }

        private bool _locked;
        public bool Locked
        {
            get { return _locked; }
            set
            {
                _locked = value;

                _windowChrome.CaptionHeight = (_locked ? 0 : WindowCaptionHeight);

                // Show the header border if the window is unlocked
                HeaderBorder.Visibility = (_locked ? Visibility.Hidden : Visibility.Visible);

                // Show and enable the window border if the window is unlocked
                BorderFull.BorderBrush = (_locked ? Brushes.Transparent : SystemColors.ActiveCaptionBrush);
                BorderFull.IsEnabled = !_locked;
            }
        }

        public MainWindow(IWindowSource windowSource)
        {
            InitializeComponent();

            // Create and set the window chrome 
            _windowChrome = new WindowChrome { CaptionHeight = WindowCaptionHeight };
            WindowChrome.SetWindowChrome(this, _windowChrome);

            // Load the window settings
            _windowSettings = WindowSettings.Load(windowSource.WindowSettings);
            _windowSettings.Name = windowSource.Name;
            _windowSettings.SetWindow(this);

            // Set the background of the border
            HeaderBorder.Background = SystemColors.ActiveCaptionBrush;

            // Configure the header label
            HeaderLabel.Foreground = SystemColors.InactiveCaptionTextBrush;
            HeaderLabel.Content = _windowSettings.Name;

            // Get the thickness so we can size the visual border
            var resizeThickness = _windowChrome.ResizeBorderThickness;

            // Set the color of the border
            BorderFull.BorderBrush = SystemColors.ActiveCaptionBrush;
            BorderFull.BorderThickness = new Thickness(resizeThickness.Left, 0, resizeThickness.Right, resizeThickness.Bottom);

            // Apply the stored settings
            _windowSettings.Apply();

            HtmlLabel.Text = "Testing";
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            _windowSettings.Location = new Point(Left, Top);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            _windowSettings.Size = new Size(Width, Height);
        }

        private List<Rect> _windowList;
        private IntPtr _windowHandle;
        protected override List<Rect> OtherWindows
        {
            get
            {
                _windowList = new List<Rect>();
                _windowHandle = new WindowInteropHelper(this).Handle;

                Functions.User32.EnumWindows(EnumWindowProc, IntPtr.Zero);

                return _windowList;
            }
        }

        private bool EnumWindowProc(IntPtr hWnd, IntPtr lParam)
        {
            var windowText = Functions.Window.GetText(hWnd);

            if (windowText == "FloatingStatusWindow" && hWnd != _windowHandle)
            {
                var windowPlacement = new Structures.WindowPlacement();
                Functions.User32.GetWindowPlacement(hWnd, ref windowPlacement);

                var p = windowPlacement.NormalPosition;

                _windowList.Add(new Rect(p.X, p.Y, p.Width, p.Height));
            }

            return true;
        }
    }
}
