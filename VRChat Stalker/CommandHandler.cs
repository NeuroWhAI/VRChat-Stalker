using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace VRChat_Stalker
{
    public class CommandHandler : ICommand
    {
        public CommandHandler(Action<object> action, Func<object, bool> condition = null)
        {
            CommandAction = action;
            CommandCondition = condition;
        }

        public Action<object> CommandAction { get; set; }
        public Func<object, bool> CommandCondition { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (CommandCondition == null)
            {
                return true;
            }

            return CommandCondition(parameter);
        }

        public void Execute(object parameter)
        {
            CommandAction(parameter);
        }
    }
}
