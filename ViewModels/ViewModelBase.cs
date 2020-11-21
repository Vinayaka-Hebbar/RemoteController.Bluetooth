using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RemoteController.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected ViewModelBase()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
