using System.Threading.Tasks;

namespace RemoteController.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Sender = new SenderViewModel();
            Receiver = new ReceiverViewModel();
        }

        public Task InitAsync()
        {
            return Task.FromResult(0);
        }

        public SenderViewModel Sender { get; }

        public ReceiverViewModel Receiver { get; }

        private Command.RelayCommand settings;
        public Command.RelayCommand Settings
        {
            get
            {
                if (settings == null)
                    settings = new Command.RelayCommand(OnSettings);
                return settings;
            }
        }

        void OnSettings()
        {
        }
    }
}
