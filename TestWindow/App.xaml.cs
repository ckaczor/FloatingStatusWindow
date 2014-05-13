using System;
using FloatingStatusWindowLibrary;
using System.Collections.Generic;
using System.Windows;
using Settings = TestWindow.Properties.Settings;

namespace TestWindow
{
    public partial class App
    {
        private List<IDisposable> _windowSourceList;

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

            _windowSourceList = new List<IDisposable>
            {
                new WindowSource1(), 
                new WindowSource2(), 
                new WindowSource3(),
                new WindowSource4()
            };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _windowSourceList.ForEach(ws => ws.Dispose());

            base.OnExit(e);
        }
    }
}
