using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.Application
{
    /// <summary>
    /// The view for a tool window that can be resized.
    /// </summary>
    public class ResizableToolWindow : UserControl
    {
        /// <summary>
        /// Creates a new instance of <see cref="ResizableToolWindow"/>.
        /// </summary>
        /// <param name="viewModel">The view model for this view.</param>
        public ResizableToolWindow(ToolWindowViewModel viewModel)
        {
            ToolViewModel = viewModel;
            DataContext = viewModel;
        }

        /// <summary>
        /// The view model for this view.
        /// </summary>
        public ToolWindowViewModel ToolViewModel { get; }

        /// <summary>
        /// A handler for the key down event on a text box that moves focus to the next control when the enter key is pressed instead of only tab.
        /// </summary>
        /// <param name="sender">The events sender.</param>
        /// <param name="e">The event arguments.</param>
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
