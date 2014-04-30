using System.Collections.Generic;
using System.Windows;

namespace TestWindow
{
    public partial class App
    {
        private List<WindowSource> _windowSourceList;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _windowSourceList = new List<WindowSource> { new WindowSource(), new WindowSource(), new WindowSource() };
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _windowSourceList.ForEach(ws => ws.Dispose());

            base.OnExit(e);
        }
    }
}
