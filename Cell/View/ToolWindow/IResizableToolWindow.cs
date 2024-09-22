using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;

namespace Cell.View.ToolWindow
{
    public interface IResizableToolWindow
    {
        double MinimumHeight { get; }

        double MinimumWidth { get; }

        Action? RequestClose { get; set; }

        List<CommandViewModel> ToolBarCommands { get; }

        ToolWindowViewModel ToolViewModel { get; }

        string ToolWindowTitle { get; }

        void HandleBeingClosed();

        void HandleBeingShown();

        bool HandleCloseRequested();
    }
}
