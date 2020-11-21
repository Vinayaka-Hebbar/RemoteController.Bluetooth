using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace RemoteController.ViewModels
{
    public sealed class TrayViewModel
    {
        public Icon Icon
        {
            get => SystemIcons.Information;
        }

        private ICommand open;
        public ICommand Open
        {
            get
            {
                if (open == null)
                    open = new Command.RelayCommand(OnOpen, CheckWindow);
                return open;
            }
        }

        private bool CheckWindow(object arg)
        {
#if Debug
            return Application.Current.MainWindow == null || !Application.Current.MainWindow.IsVisible;
#else
            return Application.Current.MainWindow == null;
#endif
        }

        private ICommand exit;

        public TrayViewModel()
        {
            Application.Current.Activated += OnActive;
        }

        private void OnActive(object sender, EventArgs e)
        {
            Window window = Application.Current.MainWindow;
            Console.WriteLine();
        }

        public ICommand Exit
        {
            get
            {
                if (exit == null)
                    exit = new Command.RelayCommand(OnExit);
                return exit;
            }
        }

        private void OnOpen(object arg)
        {
            Application.Current.MainWindow = new MainWindow();
            Application.Current.MainWindow.Show();
        }

        private void OnExit(object arg)
        {
            Application.Current.Shutdown();
        }
    }
}
