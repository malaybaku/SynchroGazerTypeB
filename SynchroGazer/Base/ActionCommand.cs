using System;
using System.Windows.Input;

namespace Baku.SynchroGazer
{
    public class ActionCommand : ICommand
    {
        public ActionCommand(Action act) : this(o => act())
        {
        }
        public ActionCommand(Action<object> act)
        {
            _act = act;
        }
        private readonly Action<object> _act;

        public void Execute(object parameter) => _act(parameter);
        public bool CanExecute(object parameter) => true;
        public event EventHandler CanExecuteChanged;
    }
}
