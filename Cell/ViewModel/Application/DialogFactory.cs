using Cell.ViewModel.ToolWindow;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// Service for creating and showing dialog windows in the application. This can be configured to use a custom dialog window implementation, which is particularly useful for unit testing.
    /// </summary>
    public class DialogFactory : DialogFactoryBase
    {

        /// <summary>
        /// The function that creates a new instance of a dialog window.
        /// </summary>
        /// <param name="title">The title to give the dialog window.</param>
        /// <param name="message">The message to display in the dialog window.</param>
        /// <param name="actions">The actions to turn into buttons for the dialog window.</param>
        /// <returns>A new dialog window.</returns>
        public override DialogWindowViewModel Create(string title, string message, List<CommandViewModel> actions)
        {
            return new DialogWindowViewModel(title, message, actions);
        }

        /// <summary>
        /// The function that shows a dialog window.
        /// </summary>
        /// <param name="dialog">The dialog to show</param>
        public override void ShowDialog(DialogWindowViewModel dialog)
        {
            ApplicationViewModel.Instance.ShowToolWindow(dialog);
        }
    }
}
