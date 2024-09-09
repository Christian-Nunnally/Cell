using Cell.Common;
using Cell.View.ToolWindow;

namespace Cell.ViewModel.Application
{
    public static class DialogFactory
    {
        public static Func<string, string, List<CommandViewModel>, IDialogWindow> DialogFactoryFunction { get; set; } = (title, message, actions) => new DialogWindow(title, message, actions);

        public static void ShowDialog(string title, string message)
        {
            var actions = new List<CommandViewModel>
            {
                new("Ok", new RelayCommand(x => { }))
            };
            var dialogWindow = DialogFactoryFunction(title, message, actions);
            dialogWindow.ShowDialog();
        }

        public static void ShowYesNoConfirmationDialog(string title, string message, Action yesAction) => ShowYesNoConfirmationDialog(title, message, yesAction, () => { });

        public static void ShowYesNoConfirmationDialog(string title, string message, Action yesAction, Action noAction)
        {
            var actions = new List<CommandViewModel>
            {
                new("Yes", new RelayCommand(x => yesAction())),
                new("No", new RelayCommand(x => noAction()))
            };
            var dialogWindow = DialogFactoryFunction(title, message, actions);
            dialogWindow.ShowDialog();
        }
    }
}
