using Cell.Model;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells.Types.Special;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cell.View.ToolWindow
{
    /// <summary>
    /// Interaction logic for EditCellPanel.xaml
    /// </summary>
    public partial class CellSettingsEditWindow : UserControl, IToolWindow
    {
        public CellSettingsEditWindow()
        {
            InitializeComponent();
        }

        public Action? RequestClose { get; set; }

        public string GetTitle()
        {
            var currentlySelectedCell = ApplicationViewModel.Instance.SheetViewModel?.SelectedCellViewModel;
            if (currentlySelectedCell is null) return "Select a cell to edit";
            return $"Cell settings editor - {currentlySelectedCell.Model.GetName()}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [];

        public bool HandleBeingClosed()
        {
            return true;
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void CreateNewColumnToTheLeftButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<ColumnCellViewModel>(sender, out var cell)) return;
            cell.AddColumnToTheLeft();
        }

        private void CreateNewColumnToTheRightButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<ColumnCellViewModel>(sender, out var cell)) return;
            cell.AddColumnToTheRight();
        }

        private void CreateNewRowAboveButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<RowCellViewModel>(sender, out var cell)) return;
            cell.AddRowAbove();
        }

        private void CreateNewRowBelowButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!ViewUtilities.TryGetSendersDataContext<RowCellViewModel>(sender, out var cell)) return;
            cell.AddRowBelow();
        }

        private void DeleteColumnButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ApplicationViewModel.Instance.SheetViewModel == null) return;
            foreach (var cell in ApplicationViewModel.Instance.SheetViewModel.CellViewModels.Where(x => x.IsSelected).OfType<ColumnCellViewModel>().ToList())
            {
                ApplicationViewModel.Instance.SheetViewModel.CellSelector.UnselectAllCells();
                cell.DeleteColumn();
            }
        }

        private void DeleteRowButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ApplicationViewModel.Instance.SheetViewModel == null) return;
            foreach (var cell in ApplicationViewModel.Instance.SheetViewModel.CellViewModels.Where(x => x.IsSelected).OfType<RowCellViewModel>().ToList())
            {
                ApplicationViewModel.Instance.SheetViewModel.CellSelector.UnselectAllCells();
                cell.DeleteRow();
            }
        }
    }
}
