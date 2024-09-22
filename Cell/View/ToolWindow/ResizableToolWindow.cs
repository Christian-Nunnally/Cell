using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public class ResizableToolWindow : UserControl, IResizableToolWindow
    {
        public ResizableToolWindow(ToolWindowViewModel viewModel)
        {
            ToolViewModel = viewModel;
            DataContext = viewModel;
        }

        public virtual double MinimumHeight { get; }

        public virtual double MinimumWidth { get; }

        public Action? RequestClose { get; set; }

        public virtual List<CommandViewModel> ToolBarCommands => [];

        public virtual string ToolWindowTitle { get; } = string.Empty;

        public ToolWindowViewModel ToolViewModel { get; }

        public virtual void HandleBeingClosed() => ToolViewModel.HandleBeingClosed();

        public virtual void HandleBeingShown() => ToolViewModel.HandleBeingShown();

        public virtual bool HandleCloseRequested() => true;
    }
}
