using RemoteController.Collection;
using RemoteController.Core;
using System.Windows.Input;

namespace RemoteController.ViewModels
{
    public sealed class ReceiverViewModel : ViewModelBase
    {
        public ObservableHashTable<string, Model.DeviceScreens> Screens { get; }

        private RemoteServer server;

        private Command.RelayCommand start;
        public ICommand Start
        {
            get
            {
                if (start == null)
                    start = new Command.RelayCommand(OnStart, CheckListener);
                return start;
            }
        }

        private Command.RelayCommand stop;

        public ReceiverViewModel()
        {
            Screens = new ObservableHashTable<string, Model.DeviceScreens>(Model.DeviceScreens.GetDeviceName);
        }

        public ICommand Stop
        {
            get
            {
                if (stop == null)
                    stop = new Command.RelayCommand(StopServer, CanStop);
                return stop;
            }
        }

        private void StopServer()
        {
            Screens.Clear();
            server.Dispose();
            server = null;
        }

        private bool CanStop()
        {
            return server != null;
        }

        public bool IsConnected => server != null;

        private bool CheckListener()
        {
            return server == null;
        }

        private void OnStart()
        {
            if (server != null)
            {
                StopServer();
            }
            server = new RemoteServer();
            ScreenConfiguration screens = server.Screens.ScreenConfiguration;
            screens.Added += ScreensAdded;
            screens.Removed += ScreensRemoved;
            server.Start();
        }

        async void ScreensRemoved(VirtualScreen screen)
        {
            if (Screens.TryGetValue(screen.Client, out Model.DeviceScreens deviceScreens)
                && await deviceScreens.RemoveScreenAsync(screen)
                && deviceScreens.IsEmpty)
            {
                Screens.Remove(screen.Client);
            }
        }

        void ScreensAdded(Direction direction, VirtualScreen screen)
        {
            if (!Screens.TryGetValue(screen.Client, out Model.DeviceScreens deviceScreens))
            {
                deviceScreens = new Model.DeviceScreens(screen.Client);
                Screens.Add(screen.Client, deviceScreens);
            }
            deviceScreens.AddScreen(screen);
        }
    }
}
