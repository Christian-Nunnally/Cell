using Cell.Core.Common;
using System.Windows.Input;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// A view model for a command, which is a named action.
    /// </summary>
    public class CommandViewModel
    {
        /// <summary>
        /// Creates a new instance of <see cref="CommandViewModel"/>.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="command">The command to execute.</param>
        public CommandViewModel(string name, Action command) : this(name, new RelayCommand(x => command()))
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="CommandViewModel"/>.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="command">The command to execute.</param>
        public CommandViewModel(string name, ICommand command)
        {
            Command = command;
            Name = name;
            ToolTip = name;
        }

        /// <summary>
        /// The command to execute.
        /// </summary>
        public ICommand Command { get; set; }

        /// <summary>
        /// Gets or sets the name of the command.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a tooltip for the command.
        /// </summary>
        public string ToolTip { get; set; }
    }
}
