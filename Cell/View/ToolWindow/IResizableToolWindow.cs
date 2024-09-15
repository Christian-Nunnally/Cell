using Cell.ViewModel.Application;

namespace Cell.View.ToolWindow
{
    internal interface IResizableToolWindow
    {
        Action? RequestClose { get; set; }

        string GetTitle();

        List<CommandViewModel> GetToolBarCommands();

        bool HandleCloseRequested();

        void HandleBeingClosed();

        void HandleBeingShown();

        double GetMinimumHeight();

        double GetMinimumWidth();
    }
}
