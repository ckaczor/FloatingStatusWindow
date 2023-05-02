using ChrisKaczor.Wpf.Windows;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;

namespace ChrisKaczor.Wpf.FloatingStatusWindow;

internal partial class MainWindow
{
    public event EventHandler LockStateChanged = delegate { };

    private const int WindowCaptionHeight = 24;

    private readonly WindowChrome _windowChrome;
    private readonly Dispatcher _dispatcher;

    public WindowSettings WindowSettings { get; set; }

    private bool _locked;

    public bool Locked
    {
        get => _locked;
        set
        {
            _locked = value;

            _windowChrome.CaptionHeight = (_locked ? 0 : WindowCaptionHeight);

            HtmlLabel.Margin = new Thickness(0, (_locked ? 0 : WindowCaptionHeight), 0, 0);

            // Show the header border if the window is unlocked
            HeaderBorder.Visibility = (_locked ? Visibility.Collapsed : Visibility.Visible);

            // Show and enable the window border if the window is unlocked
            BorderFull.BorderBrush = (_locked ? Brushes.Transparent : SystemColors.ActiveCaptionBrush);
            BorderFull.IsEnabled = !_locked;

            LockStateChanged(null, EventArgs.Empty);
        }
    }

    public MainWindow(IWindowSource windowSource)
    {
        InitializeComponent();

        _dispatcher = Dispatcher.CurrentDispatcher;

        // Create and set the window chrome 
        _windowChrome = new WindowChrome { CaptionHeight = WindowCaptionHeight };
        WindowChrome.SetWindowChrome(this, _windowChrome);

        // Load the window settings
        WindowSettings = WindowSettings.Load(windowSource.WindowSettings);
        WindowSettings.Name = windowSource.Name;
        WindowSettings.SetWindow(this);

        // Set the background of the border
        HeaderBorder.Background = SystemColors.ActiveCaptionBrush;

        // Configure the header label
        HeaderLabel.Foreground = SystemColors.InactiveCaptionTextBrush;
        HeaderLabel.Content = WindowSettings.Name;

        // Get the thickness so we can size the visual border
        var resizeThickness = _windowChrome.ResizeBorderThickness;

        // Set the color of the border
        BorderFull.BorderBrush = SystemColors.ActiveCaptionBrush;
        BorderFull.BorderThickness = new Thickness(resizeThickness.Left, 0, resizeThickness.Right, resizeThickness.Bottom);

        // Apply the stored settings
        WindowSettings.Apply();
    }

    protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WindowManager.SetLockMessage)
        {
            var lockState = (wParam == 1);

            _dispatcher.InvokeAsync(() =>
            {
                WindowSettings.Locked = lockState;
                Locked = lockState;
            });

            return IntPtr.Zero;
        }

        if (msg != WindowManager.CloseMessage)
            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);

        _dispatcher.InvokeAsync(Close);
        return IntPtr.Zero;
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);

        WindowSettings.Location = new Point(Left, Top);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        WindowSettings.Size = new Size(Width, Height);
    }

    protected override List<WindowInformation> OtherWindows
    {
        get
        {
            var windowHandle = new WindowInteropHelper(this).Handle;

            return WindowManager.GetWindowList(windowHandle);
        }
    }

    private void HandleChangeAppearanceClick(object sender, RoutedEventArgs e)
    {
        var appearanceWindow = new AppearanceWindow(WindowSettings);
        appearanceWindow.ShowDialog();
    }
}