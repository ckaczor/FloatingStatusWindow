﻿using FloatingStatusWindowLibrary;
using System;
using System.Globalization;
using System.Reflection;
using System.Timers;
using System.Windows.Threading;

namespace TestWindow
{
    internal class WindowSource2 : IWindowSource, IDisposable
    {
        private readonly FloatingStatusWindow _floatingStatusWindow;
        private readonly Timer _timer;
        private readonly Dispatcher _dispatcher;

        internal WindowSource2()
        {
            _floatingStatusWindow = new FloatingStatusWindow(this);
            _floatingStatusWindow.SetText("Loading...");

            _dispatcher = Dispatcher.CurrentDispatcher;

            _timer = new Timer(1000);
            _timer.Elapsed += HandleTimerElapsed;
            _timer.Enabled = true;
        }

        private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _dispatcher.InvokeAsync(() => _floatingStatusWindow.SetText(DateTime.Now.ToString(CultureInfo.InvariantCulture)));
        }

        public void Dispose()
        {
            _timer.Enabled = false;
            _timer.Dispose();

            _floatingStatusWindow.Save();
            _floatingStatusWindow.Dispose();
        }

        public string Name
        {
            get { return "Test Window 2"; }
        }

        public System.Drawing.Icon Icon
        {
            get { return Properties.Resources.ApplicationIcon; }
        }

        public bool HasSettingsMenu
        {
            get { return true; }
        }

        public bool HasAboutMenu => true;

        public void ShowAbout()
        {
            _floatingStatusWindow.SetText(Assembly.GetEntryAssembly().GetName().Version.ToString());
        }

        public void ShowSettings()
        {
            
        }

        public bool HasRefreshMenu
        {
            get { return true; }
        }

        public void Refresh()
        {

        }

        public string WindowSettings
        {
            get
            {
                return Properties.Settings.Default.WindowSettings2;
            }
            set
            {
                Properties.Settings.Default.WindowSettings2 = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
