
using Cell.Core.Common;
using Cell.ViewModel.ToolWindow;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// Base class for a dialog factory that creates and shows dialog windows. Subclasses can decide how to create and show dialog windows.
    /// </summary>
    public abstract class DialogFactoryBase
    {
        /// <summary>
        /// The function that creates a new instance of a dialog window.
        /// </summary>
        public abstract DialogWindowViewModel Create(string title, string message, List<CommandViewModel> actions);

        /// <summary>
        /// The function that shows a dialog window.
        /// </summary>
        public abstract void ShowDialog(DialogWindowViewModel dialog);

        /// <summary>
        /// Shows a simple dialog with a message and an "Ok" button.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message to display in the dialog.</param>
        public void Show(string title, string message)
        {
            var actions = new List<CommandViewModel>
            {
                new("Ok", new RelayCommand(x => { }))
            };
            var dialog = Create(title, message, actions);
            ShowDialog(dialog);
        }

        /// <summary>
        /// Shows a dialog with a question and a "Yes" and "No" button.
        /// </summary>
        /// <param name="title">The title to display in the dialog top bar.</param>
        /// <param name="message">The question to show in the dialog.</param>
        /// <param name="yesAction">The action to run if the user clicks yes.</param>
        public void ShowYesNo(string title, string message, Action yesAction) => ShowYesNo(title, message, yesAction, () => { });

        /// <summary>
        /// Shows a dialog with a question and a "Yes" and "No" button.
        /// </summary>
        /// <param name="title">The title to display in the dialog top bar.</param>
        /// <param name="message">The question to show in the dialog.</param>
        /// <param name="yesAction">The action to run if the user clicks yes.</param>
        /// <param name="noAction">The action to run if the user clicks no.</param>
        public void ShowYesNo(string title, string message, Action yesAction, Action noAction)
        {
            var actions = new List<CommandViewModel>
            {
                new("Yes", new RelayCommand(x => yesAction())),
                new("No", new RelayCommand(x => noAction()))
            };
            var dialogWindow = Create(title, message, actions);
            ShowDialog(dialogWindow);
        }
    }
}
