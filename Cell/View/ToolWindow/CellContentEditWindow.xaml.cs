using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class CellContentEditWindow : ResizableToolWindow
    {
        public CellContentEditWindow(CellContentEditWindowViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        public override List<CommandViewModel> ToolBarCommands => [
            new CommandViewModel("Auto-Index", IndexSelectedCells) { ToolTip = "Sets the index of selected cells in an incrementing fashion (0, 1, 2...). Will work horizontially if only one row is selected." },
            ];

        private CellContentEditWindowViewModel CellContentEditWindowViewModel => (CellContentEditWindowViewModel)ToolViewModel;

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
            CellContentEditWindowViewModel.EditPopulateFunction();
        }

        private void EditTriggerFunctionButtonClicked(object sender, RoutedEventArgs e)
        {
            CellContentEditWindowViewModel.EditTriggerFunction();
        }
    }
}
