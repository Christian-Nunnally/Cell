using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows.Controls;

namespace Cell.View.ToolWindow
{
    public partial class ImportWindow : UserControl, IResizableToolWindow
    {
        private readonly ImportWindowViewModel _viewModel;
        public ImportWindow(ImportWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = viewModel;
            InitializeComponent();
        }

        public double MinimumHeight => 200;

        public double MinimumWidth => 200;

        public Action? RequestClose { get; set; }

        public List<CommandViewModel> ToolBarCommands => [];

        public string ToolWindowTitle => "Import";

        public ToolWindowViewModel ToolViewModel => _viewModel;

        public void HandleBeingClosed()
        {
            _viewModel.HandleBeingShown();
        }

        public void HandleBeingShown()
        {
            _viewModel.HandleBeingClosed();
        }

        public bool HandleCloseRequested()
        {
            return true;
        }

        private void ImportSheetButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.ImportingTemplateName))
            {
                DialogFactory.ShowDialog("No template selected", "Please select a template to import.");
                return;
            }
            if (string.IsNullOrWhiteSpace(_viewModel.NewSheetNameForImportedTemplates))
            {
                DialogFactory.ShowDialog("No sheet name", "Please enter a name for the new sheet.");
                return;
            }
            var templateName = _viewModel.ImportingTemplateName;
            var sheetName = _viewModel.NewSheetNameForImportedTemplates;
            ApplicationViewModel.Instance.SheetTracker.ImportSheetTemplate(templateName, sheetName, _viewModel.SkipExistingCollectionsDuringImport);
        }
    }
}
