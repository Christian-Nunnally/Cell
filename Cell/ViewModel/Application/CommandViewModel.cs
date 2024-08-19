using System.Windows.Input;

namespace Cell.ViewModel.Application
{
    public class CommandViewModel(string name, ICommand command)
    {
        public ICommand Command { get; set; } = command;

        public string Name { get; set; } = name;

        public string ToolTip { get; set; } = name;
    }
}
