using RemoteController.Sockets;
using System;
using System.Windows.Input;

namespace RemoteController.ViewModels
{
    public sealed class ReceiverViewModel : ViewModelBase
    {
        private BluetoothListener listener;
        private static readonly Guid Service = new Guid("{7A51FDC2-FDDF-4c9b-AFFC-98BCD91BF93B}");

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
                    stop = new Command.RelayCommand(OnStop, CanStop);
                return stop;
            }
        }

        private void OnStop()
        {
            listener.Stop();
        }

        private bool CanStop()
        {
            return listener != null && listener.Active;
        }

        private bool CheckListener()
        {
            return listener == null || !listener.Active;
        }

        private void OnStart()
        {
            if (listener == null)
            {
                listener = new BluetoothListener(Service);
            }
            listener.Start();
        }
    }
}
