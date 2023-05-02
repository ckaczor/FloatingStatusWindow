using ChrisKaczor.Wpf.Windows.FloatingStatusWindow.Properties;
using H.NotifyIcon;
using JetBrains.Annotations;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ChrisKaczor.Wpf.Windows.FloatingStatusWindow;

[PublicAPI]
public class FloatingStatusWindow : IDisposable
{
    public event EventHandler WindowResized = delegate { };
    public event EventHandler WindowClosed = delegate { };

    private readonly MainWindow _mainWindow;
    private readonly TaskbarIcon _trayIcon;

    private readonly MenuItem _allWindowsMenuItem;
    private readonly Separator _allWindowsSeparator;

    private readonly MenuItem _lockMenuItem;
    private readonly MenuItem _autoStartMenuItem;

    private readonly IWindowSource _windowSource;

    public FloatingStatusWindow(IWindowSource windowSource)
    {
        _windowSource = windowSource;

        var contextMenu = new ContextMenu();
        contextMenu.Opened += HandleContextMenuOpened;

        MenuItem menuItem;

        if (_windowSource.HasSettingsMenu)
        {
            menuItem = new MenuItem { Header = Resources.ContextMenuSettings };
            menuItem.Click += (_, _) => _windowSource.ShowSettings();
            contextMenu.Items.Add(menuItem);

            contextMenu.Items.Add(new Separator());
        }

        if (_windowSource.HasRefreshMenu)
        {
            menuItem = new MenuItem { Header = Resources.ContextMenuRefresh };
            menuItem.Click += (_, _) => _windowSource.Refresh();
            contextMenu.Items.Add(menuItem);

            contextMenu.Items.Add(new Separator());
        }

        _allWindowsMenuItem = new MenuItem { Header = Resources.AllWindowsMenu };
        contextMenu.Items.Add(_allWindowsMenuItem);

        menuItem = new MenuItem { Header = Resources.ContextMenuLock };
        menuItem.Click += (_, _) => WindowManager.SetLockOnAll(true);
        _allWindowsMenuItem.Items.Add(menuItem);

        menuItem = new MenuItem { Header = Resources.ContextMenuUnlock };
        menuItem.Click += (_, _) => WindowManager.SetLockOnAll(false);
        _allWindowsMenuItem.Items.Add(menuItem);

        _allWindowsMenuItem.Items.Add(new Separator());

        menuItem = new MenuItem { Header = Resources.ContextMenuClose };
        menuItem.Click += (_, _) => WindowManager.CloseAll();
        _allWindowsMenuItem.Items.Add(menuItem);

        _allWindowsSeparator = new Separator();
        contextMenu.Items.Add(_allWindowsSeparator);

        var optionsMenu = new MenuItem { Name = "contextMenuItemOptions", Header = Resources.WindowMenu };
        contextMenu.Items.Add(optionsMenu);

        _lockMenuItem = new MenuItem
        {
            Name = "contextMenuItemLocked",
            IsChecked = false,
            Header = Resources.ContextMenuLocked
        };
        _lockMenuItem.Click += HandleLockedMenuItemClicked;
        optionsMenu.Items.Add(_lockMenuItem);

        if (StartManager.ManageAutoStart)
        {
            optionsMenu.Items.Add(new Separator());

            _autoStartMenuItem = new MenuItem
            {
                Name = "contextMenuItemAutoStart",
                IsChecked = StartManager.AutoStartEnabled,
                Header = Resources.ContextMenuAutoStart
            };
            _autoStartMenuItem.Click += (_, _) => StartManager.AutoStartEnabled = !StartManager.AutoStartEnabled;
            optionsMenu.Items.Add(_autoStartMenuItem);
        }

        optionsMenu.Items.Add(new Separator());

        menuItem = new MenuItem
        {
            Name = "contextMenuResetPosition",
            Header = Resources.ContextMenuResetPosition
        };
        menuItem.Click += HandleResetPositionMenuItemClick;
        optionsMenu.Items.Add(menuItem);

        menuItem = new MenuItem
        {
            Name = "contextMenuChangeAppearance",
            Header = Resources.ContextMenuChangeAppearance
        };
        menuItem.Click += HandleChangeAppearanceMenuItemClick;
        optionsMenu.Items.Add(menuItem);

        contextMenu.Items.Add(new Separator());

        if (_windowSource.HasAboutMenu)
        {
            menuItem = new MenuItem { Header = Resources.ContextMenuAbout };
            menuItem.Click += (_, _) => _windowSource.ShowAbout();
            contextMenu.Items.Add(menuItem);

            contextMenu.Items.Add(new Separator());
        }

        menuItem = new MenuItem
        {
            Name = "contextMenuItemExit",
            Header = Resources.ContextMenuExit
        };
        menuItem.Click += HandleExitMenuItemClick;
        contextMenu.Items.Add(menuItem);

        _trayIcon = new TaskbarIcon
        {
            ToolTipText = _windowSource.Name,
            Icon = _windowSource.Icon,
            ContextMenu = contextMenu,
            Id = _windowSource.Id
        };

        _trayIcon.ForceCreate();

        _mainWindow = new MainWindow(windowSource);
        _mainWindow.Closed += HandleMainWindowClosed;
        _mainWindow.SizeChanged += HandleWindowSizeChanged;
        _mainWindow.LocationChanged += HandleWindowLocationChanged;
        _mainWindow.LockStateChanged += HandleWindowLockStateChanged;

        _mainWindow.Show();
    }

    private void HandleResetPositionMenuItemClick(object sender, RoutedEventArgs e)
    {
        _mainWindow.Left = 0;
        _mainWindow.Top = 0;
        _mainWindow.Width = 300;
        _mainWindow.Height = 300;
    }

    private void HandleWindowLockStateChanged(object sender, EventArgs e)
    {
        Save();
    }

    private void HandleWindowLocationChanged(object sender, EventArgs e)
    {
        Save();
    }

    private void HandleWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
        WindowResized(this, EventArgs.Empty);

        Save();
    }

    private void HandleChangeAppearanceMenuItemClick(object sender, RoutedEventArgs e)
    {
        var appearanceWindow = new AppearanceWindow(_mainWindow.WindowSettings);
        appearanceWindow.ShowDialog();

        Save();
    }

    private void HandleMainWindowClosed(object sender, EventArgs e)
    {
        Save();

        WindowClosed(null, EventArgs.Empty);

        _trayIcon.Dispose();
    }

    public ContextMenu ContextMenu => _trayIcon.ContextMenu;

    private void HandleContextMenuOpened(object sender, RoutedEventArgs e)
    {
        _lockMenuItem.IsChecked = _mainWindow.WindowSettings.Locked;

        if (_autoStartMenuItem != null)
            _autoStartMenuItem.IsChecked = StartManager.AutoStartEnabled;

        var windowCount = WindowManager.GetWindowList().Count;

        _allWindowsMenuItem.Visibility = windowCount <= 1 ? Visibility.Collapsed : Visibility.Visible;
        _allWindowsSeparator.Visibility = _allWindowsMenuItem.Visibility;
    }

    public void SetText(string text)
    {
        _mainWindow.HtmlLabel.Text = text;
    }

    private void HandleLockedMenuItemClicked(object sender, RoutedEventArgs e)
    {
        _mainWindow.WindowSettings.Locked = !_mainWindow.WindowSettings.Locked;
        _mainWindow.WindowSettings.Apply();
    }

    private void HandleExitMenuItemClick(object sender, RoutedEventArgs e)
    {
        Save();

        _mainWindow.Close();
    }

    public void Save()
    {
        _windowSource.WindowSettings = _mainWindow.WindowSettings.Save();
    }

    public void Dispose()
    {
        _trayIcon.Dispose();

        _mainWindow.Close();

        GC.SuppressFinalize(this);
    }

    public Point Location => new(_mainWindow.Left, _mainWindow.Top);

    public Size Size => new(_mainWindow.Width, _mainWindow.Height);

    public Size ContentSize => new(_mainWindow.HtmlLabel.ActualWidth, _mainWindow.HtmlLabel.ActualHeight);

    public WindowSettings Settings => _mainWindow.WindowSettings;

    public string IconToolTipText
    {
        get => _trayIcon.ToolTipText;
        set => _trayIcon.ToolTipText = value;
    }
}