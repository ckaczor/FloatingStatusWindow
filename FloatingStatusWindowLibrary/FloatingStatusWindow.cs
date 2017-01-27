using FloatingStatusWindowLibrary.Properties;
using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace FloatingStatusWindowLibrary
{
    public class FloatingStatusWindow : IDisposable
    {
        public event EventHandler WindowResized = delegate { };
        public event EventHandler WindowClosed = delegate { };

        private readonly MainWindow _mainWindow;
        private readonly TaskbarIcon _taskbarIcon;

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
                menuItem.Click += (sender, args) => _windowSource.ShowSettings();
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());
            }

            if (_windowSource.HasRefreshMenu)
            {
                menuItem = new MenuItem { Header = Resources.ContextMenuRefresh };
                menuItem.Click += (sender, args) => _windowSource.Refresh();
                contextMenu.Items.Add(menuItem);

                contextMenu.Items.Add(new Separator());
            }

            _allWindowsMenuItem = new MenuItem { Header = Resources.AllWindowsMenu };
            contextMenu.Items.Add(_allWindowsMenuItem);

            menuItem = new MenuItem { Header = Resources.ContextMenuLock };
            menuItem.Click += (sender, args) => WindowManager.SetLockOnAll(true);
            _allWindowsMenuItem.Items.Add(menuItem);

            menuItem = new MenuItem { Header = Resources.ContextMenuUnlock };
            menuItem.Click += (sender, args) => WindowManager.SetLockOnAll(false);
            _allWindowsMenuItem.Items.Add(menuItem);

            _allWindowsMenuItem.Items.Add(new Separator());

            menuItem = new MenuItem { Header = Resources.ContextMenuClose };
            menuItem.Click += (sender, args) => WindowManager.CloseAll();
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
                _autoStartMenuItem.Click += (sender, args) => StartManager.AutoStartEnabled = !StartManager.AutoStartEnabled;
                optionsMenu.Items.Add(_autoStartMenuItem);
            }

            optionsMenu.Items.Add(new Separator());

            menuItem = new MenuItem
            {
                Name = "contextMenuChangeAppearance",
                Header = Resources.ContextMenuChangeAppearance
            };
            menuItem.Click += HandleChangeAppearancemMenuItemClick;
            optionsMenu.Items.Add(menuItem);

            contextMenu.Items.Add(new Separator());

            menuItem = new MenuItem
            {
                Name = "contextMenuItemExit",
                Header = Resources.ContextMenuExit
            };
            menuItem.Click += HandleExitMenuItemClick;
            contextMenu.Items.Add(menuItem);

            _taskbarIcon = new TaskbarIcon
            {
                ToolTipText = _windowSource.Name,
                Icon = _windowSource.Icon,
                ContextMenu = contextMenu
            };

            _mainWindow = new MainWindow(windowSource);
            _mainWindow.Closed += HandleMainWindowClosed;
            _mainWindow.SizeChanged += HandleWindowSizeChanged;
            _mainWindow.LocationChanged += HandleWindowLocationChanged;
            _mainWindow.LockStateChanged += HandleWindowLockStateChanged;

            _mainWindow.Show();
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
            WindowResized(this, new EventArgs());

            Save();
        }

        private void HandleChangeAppearancemMenuItemClick(object sender, RoutedEventArgs e)
        {
            var appearanceWindow = new AppearanceWindow(_mainWindow.WindowSettings);
            appearanceWindow.ShowDialog();

            Save();
        }

        private void HandleMainWindowClosed(object sender, EventArgs e)
        {
            Save();

            WindowClosed(null, new EventArgs());

            _taskbarIcon.Dispose();
        }

        public ContextMenu ContextMenu
        {
            get { return _taskbarIcon.ContextMenu; }
        }

        private void HandleContextMenuOpened(object sender, RoutedEventArgs e)
        {
            _lockMenuItem.IsChecked = _mainWindow.WindowSettings.Locked;

            if (_autoStartMenuItem != null)
                _autoStartMenuItem.IsChecked = StartManager.AutoStartEnabled;

            _allWindowsMenuItem.Visibility = WindowManager.GetWindowList().Count == 0 ? Visibility.Collapsed : Visibility.Visible;
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
            _taskbarIcon.Dispose();

            _mainWindow.Close();
        }

        public Point Location
        {
            get { return new Point(_mainWindow.Left, _mainWindow.Top); }
        }

        public Size Size
        {
            get { return new Size(_mainWindow.Width, _mainWindow.Height); }
        }

        public Size ContentSize
        {
            get { return new Size(_mainWindow.HtmlLabel.ActualWidth, _mainWindow.HtmlLabel.ActualHeight); }
        }

        public WindowSettings Settings
        {
            get { return _mainWindow.WindowSettings; }
        }

        public string IconToolTipText
        {
            get { return _taskbarIcon.ToolTipText; }
            set { _taskbarIcon.ToolTipText = value; }
        }
    }
}
