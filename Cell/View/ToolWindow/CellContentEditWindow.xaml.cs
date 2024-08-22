using Cell.Common;
using Cell.Persistence;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// Interaction logic for CellTextEditBar.xaml
    /// </summary>
    public partial class CellContentEditWindow : UserControl, IToolWindow
    {
        public CellContentEditWindow()
        {
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public string GetTitle()
        {
            var currentlySelectedCell = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModel;
            if (currentlySelectedCell is null) return "Select a cell to edit";
            return $"Content editor - {currentlySelectedCell.GetName()}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [
            new CommandViewModel("Auto-Index", new RelayCommand(x => IndexSelectedCells())) {ToolTip = "Sets the index of selected cells in an incrementing fashion (0, 1, 2...). Will work horizontially if only one row is selected."},
            ];

        public void HandleBeingClosed()
        {
        }

        private static void IndexSelectedCells()
        {
            var selectedCells = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModels.ToList();
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
            if (ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.PopulateFunctionName)) cell.PopulateFunctionName = "Untitled";
                var function = PluginFunctionLoader.GetOrCreateFunction("object", cell.PopulateFunctionName);
                var editor = new CodeEditorWindow(function, x =>
                {
                    function.SetUserFriendlyCode(x, cell.Model);
                    (cell as ListCellViewModel)?.UpdateList();
                }, cell.Model);
                ApplicationViewModel.Instance.MainWindow.ShowToolWindow(editor, true);
            }
        }

        private void EditTriggerFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.TriggerFunctionName)) cell.TriggerFunctionName = "Untitled";
                var function = PluginFunctionLoader.GetOrCreateFunction("void", cell.TriggerFunctionName);
                var editor = new CodeEditorWindow(function, x =>
                {
                    function.SetUserFriendlyCode(x, cell.Model);
                }, cell.Model);
                ApplicationViewModel.Instance.MainWindow.ShowToolWindow(editor, true);
            }
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
