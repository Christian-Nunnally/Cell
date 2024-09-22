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
                DialogOptions.Add(action);
            }
        }

        public override string ToolWindowTitle { get; set; }

        public ObservableCollection<CommandViewModel> DialogOptions { get; set; } = [];

        public string Message { get; set; }
    }
}
