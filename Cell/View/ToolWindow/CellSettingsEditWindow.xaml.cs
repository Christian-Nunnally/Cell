using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class CellSettingsEditWindow : ResizableToolWindow
    {
        public CellSettingsEditWindow(CellSettingsEditWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override double MinimumHeight => 200;

        public override double MinimumWidth => 200;

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
