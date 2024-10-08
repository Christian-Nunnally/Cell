﻿using Cell.Common;
using Cell.View.ToolWindow;
using Cell.ViewModel.ToolWindow;

namespace Cell.ViewModel.Application
{
    public static class DialogFactory
    {
        public static Func<string, string, List<CommandViewModel>, IDialogWindow> DialogFactoryFunction { get; set; } = (title, message, actions) => new DialogWindow(new DialogWindowViewModel(title, message, actions));

        public static Action<IDialogWindow> ShowDialogFunction { get; set; } = dialogWindow =>
            {
                if (dialogWindow is ResizableToolWindow resizableToolWindow)
                {
                    ApplicationViewModel.Instance.ShowToolWindow(resizableToolWindow);
                }
            };

        public static void ShowDialog(string title, string message)
        {
            var actions = new List<CommandViewModel>
            {
                new("Ok", new RelayCommand(x => { }))
            };
            var dialogWindow = DialogFactoryFunction(title, message, actions);
            ShowDialogFunction?.Invoke(dialogWindow);
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
            ShowDialogFunction?.Invoke(dialogWindow);
        }
    }
}
