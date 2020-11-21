using System.Windows;

namespace RemoteController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Desktop.Traybar traybar;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            traybar = (Desktop.Traybar)FindResource("SysTrayBar");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            traybar?.Dispose();
            base.OnExit(e);
        }

        private void OnTraybarContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
        {
        }
    }
}
