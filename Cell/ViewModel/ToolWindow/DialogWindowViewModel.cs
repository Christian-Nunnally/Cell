using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    /// <summary>
    /// A tool window that displays a dialog with a message and a list of actions the user can take.
    /// </summary>
    public class DialogWindowViewModel : ToolWindowViewModel
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DialogWindowViewModel"/>.
        /// </summary>
        /// <param name="title">The title to display in the top bar of the dialog window.</param>
        /// <param name="message">The message to display inside the dialog window.</param>
        /// <param name="actions">The list of actions the user can select in the dialog.</param>
        public DialogWindowViewModel(string title, string message, List<CommandViewModel> actions)
        {
            ToolWindowTitle = title;
            Message = message;
            foreach (var action in actions)
            {
                var actionWrappedWithClose = new CommandViewModel(action.Name, () =>
                {
                    action.Command.Execute(null);
                    RequestClose?.Invoke();
                });
                DialogOptions.Add(actionWrappedWithClose);
            }
        }

        /// <summary>
        /// Gets the default height of this tool window when it is shown.
        /// </summary>
        public override double DefaultHeight => 150;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 300;

        /// <summary>
        /// Gets the list of commands to display as buttons in the dialog.
        /// </summary>
        public ObservableCollection<CommandViewModel> DialogOptions { get; set; } = [];

        /// <summary>
        /// Gets or sets the message to display in the dialog.
        /// </summary>
        public string Message { get; set; }
    }
}
