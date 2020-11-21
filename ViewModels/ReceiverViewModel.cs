using RemoteController.Sockets;
using System;
using System.Windows.Input;

namespace RemoteController.ViewModels
{
    internal sealed class ReceiverViewModel : ViewModelBase
    {
        private BluetoothListener listener;
        private static readonly Guid Service = new Guid("{7A51FDC2-FDDF-4c9b-AFFC-98BCD91BF93B}");

        private ICommand start;
        public ICommand Start
        {
            get
            {
                if (start == null)
                    start = new Command.RelayCommand(OnStart, CheckListener);
                return start;
            }
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
        }
    }
}
