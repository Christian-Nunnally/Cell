using Cell.Common;
using System.Windows.Input;

namespace Cell.ViewModel.Application
{
    public class CommandViewModel
    {
        public CommandViewModel(string name, Action command) : this(name, new RelayCommand(x => command()))
        {
        }

        public CommandViewModel(string name, ICommand command)
        {
            Command = command;
            Name = name;
            ToolTip = name;
        }

        public ICommand Command { get; set; }

        public string Name { get; set; }

        public string ToolTip { get; set; }
    }
}
