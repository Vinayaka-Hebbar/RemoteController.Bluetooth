using System.Windows;

namespace RemoteController
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Desktop.Traybar traybar;

        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandled;
        }

        private void OnDispatcherUnhandled(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var res = MessageBox.Show(e.Exception.Message, "UnhandledException", MessageBoxButton.OKCancel);
            if (res == MessageBoxResult.OK)
            {
                e.Handled = true;
            }
            else
            {
                res = MessageBox.Show(e.Exception.StackTrace, "UnhandledException", MessageBoxButton.OK);
                if(res == MessageBoxResult.OK)
                {
                    Shutdown();
                }
            }
        }

        public static object MainModel => Current.TryFindResource(nameof(MainModel));

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DependencyService.Instance.Register<Services.ApplicationSettings>();
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
