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
        public ToolWindowViewModel ToolViewModel { get; }

        protected void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Shift) return;
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (sender is not TextBox textbox) return;
                textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }
    }
}
