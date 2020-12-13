using RemoteController.Core;
using System.Windows.Input;

namespace RemoteController.ViewModels
{
    public sealed class ReceiverViewModel : ViewModelBase
    {
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
            server.Start();
        }
    }
}
