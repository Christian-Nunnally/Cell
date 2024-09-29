using Cell.ViewModel.Application;
using Cell.ViewModel.ToolWindow;
using System.Windows;

namespace Cell.View.ToolWindow
{
    public partial class ImportWindow : ResizableToolWindow
    {
        private ImportWindowViewModel _viewModel;
        public ImportWindow(ImportWindowViewModel viewModel) : base(viewModel)
        {
            _viewModel = viewModel;
            InitializeComponent();
        }

        private void ImportSheetButtonClicked(object sender, RoutedEventArgs e)
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
