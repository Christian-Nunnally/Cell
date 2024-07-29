using Cell.Persistence;
using Cell.View.ToolWindow;
using Cell.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View
{
    /// <summary>
    /// Interaction logic for CellTextEditBar.xaml
    /// </summary>
    public partial class CellTextEditBar : UserControl, IToolWindow
    {
        public CellTextEditBar()
        {
            InitializeComponent();
        }

        private void TextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        private void EditPopulateFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.PopulateFunctionName)) cell.PopulateFunctionName = "Untitled";
                var function = PluginFunctionLoader.GetOrCreateFunction("object", cell.PopulateFunctionName);
                var code = function.GetUserFriendlyCode(cell.Model);
                var editor = new FloatingCodeEditor(function, code, x =>
                {
                    function.SetUserFriendlyCode(x, cell.Model);
                    (cell as ListCellViewModel)?.UpdateList();
                }, true, cell);
                ApplicationViewModel.Instance.MainWindow.ShowToolWindow(editor);
            }
        }

        private void EditTriggerFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewUtilities.TryGetSendersDataContext<CellViewModel>(sender, out var cell))
            {
                if (string.IsNullOrEmpty(cell.TriggerFunctionName)) cell.TriggerFunctionName = "Untitled";
                var function = PluginFunctionLoader.GetOrCreateFunction("void", cell.TriggerFunctionName);
                var code = function.GetUserFriendlyCode(cell.Model);
                var editor = new FloatingCodeEditor(function, code, x =>
                {
                    function.SetUserFriendlyCode(x, cell.Model);
                }, true, cell);
                ApplicationViewModel.Instance.MainWindow.ShowToolWindow(editor);
            }
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

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        public List<CommandViewModel> GetToolBarCommands() => [
            new CommandViewModel("Auto-Index", new RelayCommand(x => true, x => IndexSelectedCells())),
            ];

        public void Close()
        {
        }

        public string GetTitle()
        {
            return ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModel?.GetName() ?? "";
        }
    }
}
