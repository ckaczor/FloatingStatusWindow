using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using Common.Wpf.HtmlLabelControl;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using Point = System.Windows.Point;
using Size = System.Windows.Size;
using SystemFonts = System.Windows.SystemFonts;

namespace FloatingStatusWindowLibrary
{
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
        private HtmlLabel HtmlLabel { get; set; }

        [XmlIgnore]
        public Font Font
        {
            get { return new Font(FontName, (float) FontSize); }
        }

        internal void SetWindow(MainWindow floatingWindow)
        {
            Window = floatingWindow;
            HtmlLabel = floatingWindow.HtmlLabel;
        }

        private WindowSettings()
        {
            var allFonts = new InstalledFontCollection();
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
            HtmlLabel.HorizontalContentAlignment = HorizontalAlignment;
            HtmlLabel.VerticalContentAlignment = VerticalAlignment;

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
            var windowSettings = (WindowSettings) serializer.Deserialize(textReader);
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
}
