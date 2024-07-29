using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public class CommandViewModel(string name, ICommand command)
    {
        public string Name { get; set; } = name;

        public ICommand Command { get; set; } = command;
    }
}
