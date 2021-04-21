using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RemoteController.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private bool isBusy;

        protected ViewModelBase()
        {

        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (isBusy == value)
                    return;
                isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
