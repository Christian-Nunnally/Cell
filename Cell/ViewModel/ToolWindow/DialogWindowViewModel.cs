using Cell.ViewModel.Application;
using System.Collections.ObjectModel;

namespace Cell.ViewModel.ToolWindow
{
    public class DialogWindowViewModel : ToolWindowViewModel
    {
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
        public override double DefaultHeight => 160;

        /// <summary>
        /// Gets the default width of this tool window when it is shown.
        /// </summary>
        public override double DefaultWidth => 250;

        public override string ToolWindowTitle { get; set; }

        public ObservableCollection<CommandViewModel> DialogOptions { get; set; } = [];

        public string Message { get; set; }
    }
}
