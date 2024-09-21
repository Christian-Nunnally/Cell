using Cell.ViewModel.Application;

namespace Cell.View.ToolWindow
{
    public interface IResizableToolWindow
    {
        Action? RequestClose { get; set; }

        double GetMinimumHeight();

        double GetMinimumWidth();

        string GetTitle();

        List<CommandViewModel> GetToolBarCommands();

        void HandleBeingClosed();

        void HandleBeingShown();

        bool HandleCloseRequested();
    }
}
