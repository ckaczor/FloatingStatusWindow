using FloatingStatusWindowLibrary;
using System;
using System.Globalization;
using System.Timers;
using System.Windows.Threading;

namespace TestWindow
{
    internal class WindowSource4 : IWindowSource, IDisposable
    {
        private readonly FloatingStatusWindow _floatingStatusWindow;
        private readonly Timer _timer;
        private readonly Dispatcher _dispatcher;

        internal WindowSource4()
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
            get { return "Test Window 4"; }
        }

        public System.Drawing.Icon Icon
        {
            get { return Properties.Resources.ApplicationIcon; }
        }

        public string WindowSettings
        {
            get
            {
                return Properties.Settings.Default.WindowSettings4;
            }
            set
            {
                Properties.Settings.Default.WindowSettings4 = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}
