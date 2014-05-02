using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FloatingStatusWindowLibrary
{
    public class FloatingStatusWindow : IDisposable
    {
        private readonly MainWindow _mainWindow;
        private readonly TaskbarIcon _taskbarIcon;
        private readonly MenuItem _lockMenuItem;

        private readonly IWindowSource _windowSource;

        public FloatingStatusWindow(IWindowSource windowSource)
        {
            _windowSource = windowSource;

            var contextMenu = new ContextMenu();
            contextMenu.Opened += HandleContextMenuOpened;

            var menuItem = new MenuItem
            {
                Name = "contextMenuChangeAppearance",
                Header = Properties.Resources.ContextMenuChangeAppearance
            };
            menuItem.Click += HandleChangeAppearancemMenuItemClick;
            contextMenu.Items.Add(menuItem);

            contextMenu.Items.Add(new Separator());

            _lockMenuItem = new MenuItem
            {
                Name = "contextMenuItemLocked",
                IsChecked = false,
                Header = Properties.Resources.ContextMenuLocked
            };
            _lockMenuItem.Click += HandleLockedMenuItemClicked;
            contextMenu.Items.Add(_lockMenuItem);

            contextMenu.Items.Add(new Separator());

            menuItem = new MenuItem
            {
                Name = "contextMenuItemExit",
                Header = Properties.Resources.ContextMenuExit
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

            _mainWindow.Show();
        }

        void HandleChangeAppearancemMenuItemClick(object sender, RoutedEventArgs e)
        {
            var appearanceWindow = new AppearanceWindow(_mainWindow.WindowSettings);
            appearanceWindow.ShowDialog();
        }

        private void HandleMainWindowClosed(object sender, EventArgs e)
        {
            _taskbarIcon.Dispose();
        }

        public ContextMenu ContextMenu
        {
            get { return _taskbarIcon.ContextMenu; }
        }

        private void HandleContextMenuOpened(object sender, RoutedEventArgs e)
        {
            _lockMenuItem.IsChecked = _mainWindow.WindowSettings.Locked;
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
    }
}
