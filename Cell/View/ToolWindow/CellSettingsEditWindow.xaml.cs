using Cell.Model;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class CellSettingsEditWindow : ResizableToolWindow
    {
        private CellSettingsEditWindowViewModel CellSettingsEditWindowViewModel => (CellSettingsEditWindowViewModel)ToolViewModel;
        public CellSettingsEditWindow(CellSettingsEditWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override double MinimumHeight => 200;

        public override double MinimumWidth => 200;

        public override string ToolWindowTitle
        {
            get
            {
                var currentlySelectedCell = CellSettingsEditWindowViewModel.CellsBeingEdited.FirstOrDefault();
                if (currentlySelectedCell is null) return "Select a cell to edit";
                return $"Cell settings editor - {currentlySelectedCell.GetName()}";
            }
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }
    }
}
