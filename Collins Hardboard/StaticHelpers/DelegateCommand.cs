using System;
using System.Windows.Input;

namespace StaticHelpers
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _action;
        private readonly Action<object> _paramAction;

        public DelegateCommand(Action action)
        {
            _action = action;
        }
        public DelegateCommand(Action<object> action)
        {
            _paramAction = action;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
            _paramAction?.Invoke(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67
    }
}
