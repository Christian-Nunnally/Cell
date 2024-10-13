using Cell.Core.Common;
using Cell.ViewModel.ToolWindow;

namespace Cell.ViewModel.Application
{
    /// <summary>
    /// Service for creating and showing dialog windows in the application. This can be configured to use a custom dialog window implementation, which is particularly useful for unit testing.
    /// </summary>
    public static class DialogFactory
    {
        /// <summary>
        /// The function that creates a new instance of a dialog window.
        /// </summary>
        public static Func<string, string, List<CommandViewModel>, DialogWindowViewModel> DialogFactoryFunction { get; set; } = (title, message, actions) => new DialogWindowViewModel(title, message, actions);

        /// <summary>
        /// The function that shows a dialog window.
        /// </summary>
        public static Action<DialogWindowViewModel> ShowDialogFunction { get; set; } = dialogWindow => ApplicationViewModel.Instance.ShowToolWindow(dialogWindow);

        /// <summary>
        /// Shows a simple dialog with a message and an "Ok" button.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="message">The message to display in the dialog.</param>
        public static void ShowDialog(string title, string message)
        {
            var actions = new List<CommandViewModel>
            {
                new("Ok", new RelayCommand(x => { }))
            };
            var dialogWindow = DialogFactoryFunction(title, message, actions);
            ShowDialogFunction?.Invoke(dialogWindow);
        }

        /// <summary>
        /// Shows a dialog with a question and a "Yes" and "No" button.
        /// </summary>
        /// <param name="title">The title to display in the dialog top bar.</param>
        /// <param name="message">The question to show in the dialog.</param>
        /// <param name="yesAction">The action to run if the user clicks yes.</param>
        public static void ShowYesNoConfirmationDialog(string title, string message, Action yesAction) => ShowYesNoConfirmationDialog(title, message, yesAction, () => { });

        /// <summary>
        /// Shows a dialog with a question and a "Yes" and "No" button.
        /// </summary>
        /// <param name="title">The title to display in the dialog top bar.</param>
        /// <param name="message">The question to show in the dialog.</param>
        /// <param name="yesAction">The action to run if the user clicks yes.</param>
        /// <param name="noAction">The action to run if the user clicks no.</param>
        public static void ShowYesNoConfirmationDialog(string title, string message, Action yesAction, Action noAction)
        {
            var actions = new List<CommandViewModel>
            {
                new("Yes", new RelayCommand(x => yesAction())),
                new("No", new RelayCommand(x => noAction()))
            };
            var dialogWindow = DialogFactoryFunction(title, message, actions);
            ShowDialogFunction?.Invoke(dialogWindow);
        }
    }
}
