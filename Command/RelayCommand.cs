using System;
using System.Windows.Input;

namespace RemoteController.Command
{
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object> onExecute;
        private readonly Func<object, bool> canExecute;

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
            return canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            onExecute(parameter);
        }
    }
}
