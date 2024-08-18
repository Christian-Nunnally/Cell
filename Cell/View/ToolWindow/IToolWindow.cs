using Cell.ViewModel;

namespace Cell.View.ToolWindow
{
    internal interface IToolWindow
    {
        public Action? RequestClose { get; set; }

        string GetTitle();

        List<CommandViewModel> GetToolBarCommands();

        void HandleBeingClosed();
    }
}
