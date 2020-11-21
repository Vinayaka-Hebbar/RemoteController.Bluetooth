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

        public async Task InitAsync()
        {
            await Sender.InitAsync();
        }

        public SenderViewModel Sender { get; }

        public ReceiverViewModel Receiver { get; }
    }
}
