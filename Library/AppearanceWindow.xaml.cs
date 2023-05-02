using System.Linq;
using System.Windows.Media;

namespace ChrisKaczor.Wpf.Windows.FloatingStatusWindow;

internal partial class AppearanceWindow
{
    private WindowSettings _currentSettings;

    private readonly WindowSettings _originalSettings;

    public AppearanceWindow(WindowSettings windowSettings)
    {
        InitializeComponent();

        _currentSettings = windowSettings;

        _originalSettings = (WindowSettings)_currentSettings.Clone();

        var allFonts = Fonts.SystemFontFamilies.OrderBy(x => x.Source);

        FontNameCombo.ItemsSource = allFonts;

        DataContext = _currentSettings;
    }

    private void HandleOkayButtonClick(object sender, System.Windows.RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void HandleWindowSourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
    {
        _currentSettings.Apply();
    }

    private void HandleWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DialogResult == true)
            return;

        _currentSettings = _originalSettings;
        _currentSettings.Apply();
    }
}