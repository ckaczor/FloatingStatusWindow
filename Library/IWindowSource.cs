using System.Drawing;

namespace FloatingStatusWindowLibrary
{
    public interface IWindowSource
    {
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
}
