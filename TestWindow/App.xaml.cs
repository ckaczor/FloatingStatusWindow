using FloatingStatusWindowLibrary;
using System.Collections.Generic;
using System.Windows;
using Settings = TestWindow.Properties.Settings;

namespace TestWindow
{
    public partial class App
    {
        private List<WindowSource> _windowSourceList;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            StartManager.ManageAutoStart = true;
            StartManager.AutoStartEnabled = Settings.Default.AutoStart;
            StartManager.AutoStartChanged += (value =>
            {
                Settings.Default.AutoStart = value;
                Settings.Default.Save();
            });

            _windowSourceList = new List<WindowSource>
            {
                //new WindowSource(), 
                //new WindowSource(), 
                new WindowSource()
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _windowSourceList.ForEach(ws => ws.Dispose());

            base.OnExit(e);
        }
    }
}
