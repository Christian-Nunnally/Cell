using Cell.Persistence;
using Cell.View.ToolWindow;
using Cell.ViewModel.Application;
using Cell.ViewModel.Cells;
using Cell.ViewModel.Cells.Types.Special;
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
            var currentlySelectedCell = ApplicationViewModel.Instance.SheetViewModel.SelectedCellViewModel;
            if (currentlySelectedCell is null) return "Select a cell to edit";
            return $"Cell settings editor - {currentlySelectedCell.GetName()}";
        }

        public List<CommandViewModel> GetToolBarCommands() => [];

        public void HandleBeingClosed() { }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textbox) textbox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void ExportSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is not CellViewModel cell) return;
            PersistenceManager.ExportSheet(cell.Model.SheetName);
            DialogWindow.ShowDialog("Sheet exported", $"The sheet has been exported to the default export location. ({PersistenceManager.CurrentTemplatePath})");
        }

        private void ImportSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is not CornerCellViewModel cell) return;
            if (cell.ImportingTemplateName == string.Empty)
            {
                DialogWindow.ShowDialog("No template selected", "Please select a template to import.");
                return;
            }
            if (string.IsNullOrWhiteSpace(cell.NewSheetNameForImportedTemplates))
            {
                DialogWindow.ShowDialog("No sheet name", "Please enter a name for the new sheet.");
                return;
            }
            PersistenceManager.ImportSheet(cell.ImportingTemplateName, cell.NewSheetNameForImportedTemplates);
        }
    }
}
