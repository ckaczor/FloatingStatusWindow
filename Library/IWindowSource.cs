using System;
using System.Drawing;

namespace ChrisKaczor.Wpf.Windows.FloatingStatusWindow;

public interface IWindowSource
{
    Guid Id { get; }
    string Name { get; }
    string WindowSettings { get; set; }
    Icon Icon { get; }

    bool HasSettingsMenu { get; }
    bool HasRefreshMenu { get; }
    bool HasAboutMenu { get; }

    void ShowSettings();
    void Refresh();
    void ShowAbout();
}