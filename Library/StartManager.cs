using ChrisKaczor.Wpf.Application;
using System;
using System.Windows;

namespace ChrisKaczor.Wpf.Windows.FloatingStatusWindow;

public static class StartManager
{
    public delegate void AutoStartChangedEventHandler(bool autoStart);

    public static event AutoStartChangedEventHandler AutoStartChanged = delegate { };

    public static bool ManageAutoStart { get; set; }

    private static bool _autoStartEnabled;

    public static bool AutoStartEnabled
    {
        get => ManageAutoStart && _autoStartEnabled;
        set
        {
            if (!ManageAutoStart)
                throw new InvalidOperationException("Cannot set AutoStartEnabled when ManageAutoStart is False");

            _autoStartEnabled = value;

            System.Windows.Application.Current.SetStartWithWindows(_autoStartEnabled);
            AutoStartChanged(_autoStartEnabled);
        }
    }
}