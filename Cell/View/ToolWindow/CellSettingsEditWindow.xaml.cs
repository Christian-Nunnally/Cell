using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class CellSettingsEditWindow : UserControl, IResizableToolWindow
    {
        private readonly CellSettingsEditWindowViewModel _viewModel;
        public CellSettingsEditWindow(CellSettingsEditWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 200;

        public double GetMinimumWidth() => 200;

        public string GetTitle()
        {
            var currentlySelectedCell = _viewModel.CellsBeingEdited.FirstOrDefault();
            if (currentlySelectedCell is null) return "Select a cell to edit";
            return $"Cell settings editor - {currentlySelectedCell.GetName()}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [];

        public bool HandleCloseRequested()
        {
            return true;
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        public void HandleBeingClosed()
        {
        }

        public void HandleBeingShown()
        {
        }
    }
}
