using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public class ResizableToolWindow : UserControl
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

        public ToolWindowViewModel ToolViewModel { get; }

        public virtual void HandleBeingClosed() => ToolViewModel.HandleBeingClosed();

        public virtual void HandleBeingShown() => ToolViewModel.HandleBeingShown();

        public virtual bool HandleCloseRequested() => true;

        protected void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Shift) return;
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (sender is not TextBox textbox) return;
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }
    }
}
