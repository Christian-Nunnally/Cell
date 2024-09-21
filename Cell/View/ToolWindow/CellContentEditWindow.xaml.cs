using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    public partial class CellContentEditWindow : UserControl, IResizableToolWindow
    {
        private readonly CellContentEditWindowViewModel _viewModel;
        public CellContentEditWindow(CellContentEditWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public double GetMinimumHeight() => 200;

        public double GetMinimumWidth() => 600;

        public string GetTitle()
        {
            var currentlySelectedCell = ApplicationViewModel.Instance.SheetViewModel?.SelectedCellViewModel;
            if (currentlySelectedCell is null) return "Select a cell to edit";
            return $"Content editor - {currentlySelectedCell.Model.GetName()}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [
            new CommandViewModel("Auto-Index", IndexSelectedCells) { ToolTip = "Sets the index of selected cells in an incrementing fashion (0, 1, 2...). Will work horizontially if only one row is selected." },
            ];

        public void HandleBeingClosed()
        {
        }

        public void HandleBeingShown()
        {
        }

        public bool HandleCloseRequested()
        {
            return true;
        }

        private static void IndexSelectedCells()
        {
            if (ApplicationViewModel.Instance.SheetViewModel == null) return;
            var selectedCells = ApplicationViewModel.Instance.SheetViewModel.CellSelector.SelectedCells.ToList();
            var leftmost = selectedCells.Select(x => x.Column).Min();
            var topmost = selectedCells.Select(x => x.Row).Min();
            var topLeftCell = selectedCells.FirstOrDefault(x => x.Row == topmost && x.Column == leftmost);
            if (topLeftCell is null) return;
            var isLinearSelection = selectedCells.Select(x => x.Column).Distinct().Count() == 1 || selectedCells.Select(x => x.Row).Distinct().Count() == 1;
            foreach (var selectedCell in selectedCells)
            {
                if (selectedCell == topLeftCell) continue;
                var distance = isLinearSelection
                    ? (selectedCell.Column - topLeftCell.Column) + (selectedCell.Row - topLeftCell.Row)
                    : selectedCell.Row - topLeftCell.Row;
                selectedCell.Index = topLeftCell.Index + distance;
            }
        }

        private void EditPopulateFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.EditPopulateFunction();
        }

        private void EditTriggerFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            _viewModel.EditTriggerFunction();
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }
    }
}
