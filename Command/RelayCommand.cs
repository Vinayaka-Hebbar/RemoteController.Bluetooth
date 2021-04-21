using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteController.Command
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object> onExecute;
        private readonly Func<object, bool> canExecute;
        private bool isBusy;

        public RelayCommand(Action onExecute) : this(onExecute, () => true)
        {
        }

        public RelayCommand(Action onExecute, Func<bool> canExecute)
        {
            this.onExecute = d => onExecute();
            this.canExecute = d => canExecute();
        }

        public RelayCommand(Action<object> onExecute) : this(onExecute, d => true) { }

        public RelayCommand(Action<object> onExecute, Func<object, bool> canExecute)
        {
            this.onExecute = onExecute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return !isBusy && canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            isBusy = true;
            try
            {
                onExecute(parameter);
            }
            finally
            {
                isBusy = false;
            }

        }
    }

    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> onExecute;
        private readonly Func<object, bool> canExecute;
        private bool isBusy;

        public AsyncRelayCommand(Action onExecute) : this(onExecute, () => true)
        {
        }

        public AsyncRelayCommand(Action onExecute, Func<bool> canExecute)
        {
            this.onExecute = d =>
            {
                onExecute();
                return Task.FromResult(0);
            };
            this.canExecute = d => canExecute();
        }

        public AsyncRelayCommand(Func<object, Task> onExecute) : this(onExecute, d => true) { }

        public AsyncRelayCommand(Func<object, Task> onExecute, Func<object, bool> canExecute)
        {
            this.onExecute = onExecute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return !isBusy && canExecute(parameter);
        }

        public async void Execute(object parameter)
        {
            isBusy = true;
            try
            {
                await onExecute(parameter);
            }
            finally
            {
                isBusy = false;
            }

        }
    }
}
