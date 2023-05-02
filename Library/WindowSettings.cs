using ChrisKaczor.Wpf.Controls;
using JetBrains.Annotations;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Serialization;

namespace ChrisKaczor.Wpf.Windows.FloatingStatusWindow;

[PublicAPI]
public class WindowSettings : ICloneable
{
    private const string DefaultFontName = "Consolas";
    private const int DefaultFontSize = 14;

    public string Name { get; set; }
    public bool Visible { get; set; }
    public Point Location { get; set; }
    public Size Size { get; set; }
    public int Padding { get; set; }
    public HorizontalAlignment HorizontalAlignment { get; set; }
    public VerticalAlignment VerticalAlignment { get; set; }
    public bool Locked { get; set; }
    public string FontName { get; set; }
    public double FontSize { get; set; }
    public Color FontColor { get; set; }

    [XmlIgnore]
    private MainWindow Window { get; set; }

    [XmlIgnore]
    private HtmlTextBlock HtmlLabel { get; set; }

    [XmlIgnore]
    private Grid ContentGrid { get; set; }

    [XmlIgnore]
    public System.Drawing.Font Font => new(FontName, (float)FontSize);

    internal void SetWindow(MainWindow floatingWindow)
    {
        Window = floatingWindow;
        HtmlLabel = floatingWindow.HtmlLabel;
        ContentGrid = floatingWindow.ContentGrid;
    }

    private WindowSettings()
    {
        var allFonts = new System.Drawing.Text.InstalledFontCollection();
        var fontExists = allFonts.Families.Any(f => f.Name == DefaultFontName);

        FontName = fontExists ? DefaultFontName : SystemFonts.MessageFontFamily.Source;
        FontColor = (System.Drawing.SystemColors.Desktop.GetBrightness() < 0.5 ? Colors.Silver : Colors.Black);
        FontSize = fontExists ? DefaultFontSize : SystemFonts.MessageFontSize;
        Padding = 5;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Bottom;
        Locked = false;
    }

    internal void Apply()
    {
        // Configure the text label
        HtmlLabel.FontFamily = new FontFamily(FontName);
        HtmlLabel.FontSize = FontSize;
        HtmlLabel.Foreground = new SolidColorBrush(FontColor);
        HtmlLabel.Padding = new Thickness(Padding);

        ContentGrid.HorizontalAlignment = HorizontalAlignment;
        ContentGrid.VerticalAlignment = VerticalAlignment;

        // Put the window in its last position
        Window.Left = Location.X;
        Window.Top = Location.Y;

        // Set the last size if we have a valid size
        if (!Size.Width.Equals(0) && !Size.Height.Equals(0))
        {
            Window.Width = Size.Width;
            Window.Height = Size.Height;
        }

        Window.Locked = Locked;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }

    internal static WindowSettings Load(string settings)
    {
        if (string.IsNullOrEmpty(settings))
            return new WindowSettings();

        var serializer = new XmlSerializer(typeof(WindowSettings));
        TextReader textReader = new StringReader(settings);
        var windowSettings = (WindowSettings)serializer.Deserialize(textReader);
        textReader.Close();

        return windowSettings;
    }

    internal string Save()
    {
        var builder = new StringBuilder();

        var serializer = new XmlSerializer(typeof(WindowSettings));
        TextWriter textWriter = new StringWriter(builder);
        serializer.Serialize(textWriter, this);
        textWriter.Close();

        return builder.ToString();
    }
}