﻿using ChrisKaczor.Wpf.FloatingStatusWindow;
using System;
using System.Globalization;
using System.Reflection;
using System.Timers;
using System.Windows.Threading;

namespace TestWindow;

internal class WindowSource3 : IWindowSource, IDisposable
{
    private readonly FloatingStatusWindow _floatingStatusWindow;
    private readonly Timer _timer;
    private readonly Dispatcher _dispatcher;

    internal WindowSource3()
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

    public Guid Id => Guid.Parse("CF7466DF-8980-452B-B2FA-290B26204BF2");

    public string Name
    {
        get { return "Test Window 3"; }
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
        get { return Properties.Settings.Default.WindowSettings3; }
        set
        {
            Properties.Settings.Default.WindowSettings3 = value;
            Properties.Settings.Default.Save();
        }
    }
}