using Cell.ViewModel;

namespace Cell.View.ToolWindow
{
    internal interface IToolWindow
    {
        void HandleBeingClosed();

        string GetTitle();

        List<CommandViewModel> GetToolBarCommands();

        public Action? RequestClose { get; set; }
    }
}
