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

        void ShowSettings();
        void Refresh();
    }
}
